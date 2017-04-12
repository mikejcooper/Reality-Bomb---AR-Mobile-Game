using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientLifecycle;
using ClientNetworking;
using UnityEngine.SceneManagement;
using GlobalNetworking;
using NetworkCompat;

public class ClientSceneManager : MonoBehaviour
{

	public NetworkLobbyPlayer LobbyPlayerPrefab;
	public GameObject GamePlayerPrefab;
	public GameObject WorldMesh { get { return _meshTransferManager.ProduceGameObject (); }}

	private DiscoveryClient _discoveryClient;
	private GameLobbyManager _networkLobbyManager;
	private PlayerDataManager _playerDataManager;
    private MeshTransferManager _meshTransferManager;
	private Process _innerProcess;
	private string _currentScene = "Idle";
	private static ClientSceneManager _instance;
	private int _defaultSleepTimeout;

	public static ClientSceneManager Instance { get { return _instance; } }


	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;

			_innerProcess = new Process ();	
		}
	}

	void Start ()
	{
		DontDestroyOnLoad (gameObject);
		var ugly = UnityThreadHelper.Dispatcher;

		_discoveryClient = transform.gameObject.AddComponent<DiscoveryClient> ();
		_networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();
		_playerDataManager = new PlayerDataManager (_networkLobbyManager);
		_meshTransferManager = new MeshTransferManager();

		_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		_networkLobbyManager.showLobbyGUI = false;

		_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

		_defaultSleepTimeout = Screen.sleepTimeout;

		List<string> lobbyScenes = new List<string> ();
		lobbyScenes.Add ("Idle");
		lobbyScenes.Add ("Leaderboard");
		_networkLobbyManager.lobbyScenes = lobbyScenes;

		_networkLobbyManager.playScene = "Game";

		InitialisePlayerCar (GamePlayerPrefab);

		_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
		_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

		// register listeners for when players connect / disconnect
		_networkLobbyManager.OnLobbyClientConnectedEvent += OnUserConnectedToGame;
		_networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserDisconnectedToGame;
		_networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserRequestLeaveGame;
		_networkLobbyManager.OnMeshClearToDownloadEvent += _meshTransferManager.FetchData;

        //Listener for when the we have finished downloading the mesh
		_meshTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;

		_discoveryClient.serverDiscoveryEvent += OnServerDiscovered;

		SceneManager.sceneLoaded += OnSceneLoaded;
	}







	private void InitialisePlayerCar (GameObject GamePlayerPrefab){

		List<List<Color>> CarColours = new List<List<Color>>();
		// Spoiler, Side glow, Blades, Body, Blades Inner, Winscreen
		CarColours.Add( new List<Color> { Color.black, new Color(6, 167, 170), new Color(128, 128, 128), new Color(96, 0, 0),  new Color(6, 167, 170), Color.black} );

		List<Color> colours = CarColours[0];

		Material[] materials = GamePlayerPrefab.transform.FindChild("Car_Model").GetComponent<MeshRenderer> ().sharedMaterials;

		materials [0].color = colours [0];
		materials [1].color = colours [1];
		materials [2].color = colours [2];
		materials [3].color = colours [3];
		materials [4].color = colours [4];
		materials [5].color = colours [5];
	}

    private void OnMeshDataReceived()
    {
		Debug.Log("OnMeshDataReceived");

		_networkLobbyManager.SetReady ();

    }

	// todo: move all current scene assignments here
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.name == "Game") {
			_currentScene = "Game";
			OnServerGameReady ();
		} else if (scene.name == "Leaderboard") {
			_currentScene = "Leaderboard";
			OnServerGameEnd ();
		}
	}

	public void OnUserRequestFindGame () {
		Debug.Log ("OnUserRequestFindGame");
		_innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		// begin listening
		_discoveryClient.ListenForServers ();

	}

	private void OnServerDiscovered (string address) {
		Debug.Log ("OnServerDiscovered");
		_innerProcess.MoveNext (Command.ConnectGame);
		ensureCorrectScene ();

		// stop listening for broadcasts
		if (_discoveryClient != null) {
        	_discoveryClient.StopBroadcast();
        }

		if (NetworkConstants.FORCE_LOCALHOST) {
			_networkLobbyManager.networkAddress = "localhost";
		} else {
			_networkLobbyManager.networkAddress = address;
		}

		_networkLobbyManager.networkPort = NetworkConstants.GAME_PORT;
		_networkLobbyManager.StartClient ();
	}

	private void OnUserConnectedToGame () {
		Debug.Log ("OnUserConnectedToGame");
		_innerProcess.MoveNext (Command.JoinedGame);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		ensureCorrectScene ();
	}

	private void OnUserDisconnectedToGame () {
		Debug.Log ("OnUserDisconnectedToGame");
		// we don't have a state change for this... yet
		Screen.sleepTimeout = _defaultSleepTimeout;
	}

	private void OnServerGameReady () {
		Debug.Log ("OnServerGameReady");
		_innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	//call at end of GameManager (client)
	public void OnGameLoaded () {
		Debug.Log ("Notifying server that we have finished loading game");
		_networkLobbyManager.SetGameLoaded ();
	}

	private void OnServerGameEnd () {
		Debug.Log ("OnServerGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	public void OnUserRequestPlaySandbox () {
		Debug.Log ("OnUserRequestPlaySandbox");
		_innerProcess.MoveNext (Command.PlaySandbox);
		ensureCorrectScene ();
	}

	public PlayerDataManager.PlayerData GetPlayerDataById (int serverId) {
		return _playerDataManager.GetPlayerById (serverId);
	}

	public PlayerDataManager.PlayerData GetThisPlayerData () {
		return _playerDataManager.GetThisPlayer();
	}

	public void OnUserRequestLeaveGame () {
		Debug.Log ("OnUserRequestLeaveGame");
		_innerProcess.MoveNext (Command.LeaveGame);

		_networkLobbyManager.StopClient ();


		ensureCorrectScene ();
	}


	private void ensureCorrectScene ()
	{
		switch (_innerProcess.CurrentState) {
		case ProcessState.Idle:
		case ProcessState.Searching:
		case ProcessState.Connecting:
			if (_currentScene != "Idle") {
				_currentScene = "Idle";
				SceneManager.LoadScene ("Idle");
			}
			break;
		case ProcessState.Sandbox:
			if (_currentScene != "Sandbox") {
				_currentScene = "Sandbox";
				SceneManager.LoadScene ("Sandbox");
			}
			break;
		case ProcessState.PlayingGame:
			// this is managed for us by LobbyNetworkManager
			break;
		case ProcessState.Leaderboard:
			// this is managed for us by LobbyNetworkManager
			break;
		}
	}

}
