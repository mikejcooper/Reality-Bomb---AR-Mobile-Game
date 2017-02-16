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
	public GameResults LastGameResults;

	private DiscoveryClient _discoveryClient;
	private GameLobbyManager _networkLobbyManager;
    private DataTransferManager _dataTransferManager;
	private Process _innerProcess;
	private string _currentScene = "Idle";
	private static ClientSceneManager _instance;

	public static ClientSceneManager Instance { get { return _instance; } }


	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}

	void Start ()
	{
		DontDestroyOnLoad (gameObject);
		_innerProcess = new Process ();	
		_discoveryClient = transform.gameObject.AddComponent<DiscoveryClient> ();
		_networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();
        _dataTransferManager = new DataTransferManager();

		_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		_networkLobbyManager.showLobbyGUI = false;

		_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

		List<string> lobbyScenes = new List<string> ();
		lobbyScenes.Add ("Idle");
		lobbyScenes.Add ("Leaderboard");
		_networkLobbyManager.lobbyScenes = lobbyScenes;

		_networkLobbyManager.playScene = "Game";

		_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
		_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

		// register listeners for when players connect / disconnect
		_networkLobbyManager.OnLobbyClientConnectedEvent += OnUserConnectedToGame;
		_networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserRequestLeaveGame;
        _networkLobbyManager.OnMeshClearToDownloadEvent += _dataTransferManager.fetchData;

        //Listener for when the we have finished downloading the mesh
        _dataTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;

		_discoveryClient.serverDiscoveryEvent += OnServerDiscovered;

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

    private void OnMeshDataReceived()
    {
        DebugConsole.Log("OnMeshDataReceived");

        //Handle scene transition?
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
		DebugConsole.Log ("OnUserRequestFindGame");
		_innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		// begin listening
		_discoveryClient.ListenForServers ();

	}

	private void OnServerDiscovered (string address) {
		DebugConsole.Log ("OnServerDiscovered");
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
		DebugConsole.Log ("OnUserConnectedToGame");
		_innerProcess.MoveNext (Command.JoinedGame);
		ensureCorrectScene ();
	}

	private void OnServerGameReady () {
		DebugConsole.Log ("OnServerGameReady");
		_innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	private void OnServerGameEnd () {
		DebugConsole.Log ("OnServerGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	public void OnUserRequestPlayMinigame () {
		DebugConsole.Log ("OnUserRequestPlayMinigame");
		_innerProcess.MoveNext (Command.PlayMinigame);
		ensureCorrectScene ();
	}

	public void OnUserRequestLeaveGame () {
		DebugConsole.Log ("OnUserRequestLeaveGame");
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
		case ProcessState.MiniGame:
			if (_currentScene != "MiniGame") {
				_currentScene = "MiniGame";
				SceneManager.LoadScene ("MiniGame");
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
