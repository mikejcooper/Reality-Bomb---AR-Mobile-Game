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
    static bool DEBUG = false;

	public delegate void OnCountDownTimeUpdateCallback (int remainingTime);
	public delegate void OnCountDownCanceledCallback (string reason);

	public OnCountDownTimeUpdateCallback OnCountDownTimeUpdateEvent = delegate {};
	public OnCountDownCanceledCallback OnCountDownCanceledEvent = delegate {};
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
	private Coroutine _countdownCoroutine;

	private string _clientNickName;

	public static ClientSceneManager Instance { get { return _instance; } }

	public ProcessState CurrentState () {
		return _innerProcess.CurrentState;
	}

	public bool StartMuted;

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

		_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
		_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

		// register listeners for when players connect / disconnect
		_networkLobbyManager.OnLobbyClientConnectedEvent += OnUserConnectedToGame;
		_networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserDisconnectedToGame;
		_networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserRequestLeaveGame;
		_networkLobbyManager.OnMeshClearToDownloadEvent += _meshTransferManager.FetchData;
		_networkLobbyManager.OnStartGameCountdownEvent += OnStartGameCountdown;
		_networkLobbyManager.OnCancelGameCountdownEvent += OnCancelGameCountdown;
		_networkLobbyManager.OnPlayerIDEvent += OnPlayerID;

        //Listener for when the we have finished downloading the mesh
		_meshTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;

		_discoveryClient.serverDiscoveryEvent += OnServerDiscovered;

		SceneManager.sceneLoaded += OnSceneLoaded;

		if (StartMuted) {
			AudioListener.volume = 0.0f;
		}
	}
		
    private void OnMeshDataReceived()
    {
		if (DEBUG) Debug.Log("OnMeshDataReceived");

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

	public void OnUserRequestFindGame (string nickname) {
		if (DEBUG) Debug.Log ("OnUserRequestFindGame");
		_clientNickName = nickname;
		_innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		if (Flags.GAME_SERVER_IS_LOCALHOST) {
			OnServerDiscovered ("localhost");
		} else {
			// begin listening
			_discoveryClient.ListenForServers ();
		}

	}

	private void OnPlayerID (int playerID) {
		// We don't actually care what our ID is.
		// This is just a good sign that the server 
		// has setup our player data and we can submit
		// our name.
		_networkLobbyManager.SendOwnPlayerName(_clientNickName);
	}

	private void OnStartGameCountdown (int delay) {
		Debug.Log (string.Format ("Starting game countdown. Game will start in {0} seconds", delay));
		_countdownCoroutine = StartCoroutine(Countdown(delay, 1f));
	}

	IEnumerator Countdown(int remainingTime, float delayTime)
	{
		OnCountDownTimeUpdateEvent (remainingTime);
		yield return new WaitForSeconds(delayTime);
		int newRemaining = (int) Mathf.Round(remainingTime - delayTime);
		if (newRemaining > 0) {
			_countdownCoroutine = StartCoroutine(Countdown(newRemaining, delayTime));
		}
	}

	private void OnCancelGameCountdown (string reason) {
		Debug.Log (string.Format ("Cancelling game countodwn because '{0}'", reason));
		StopCoroutine (_countdownCoroutine);
		OnCountDownCanceledEvent (reason);
	}

	private void OnServerDiscovered (string address) {
        if (DEBUG) Debug.Log ("OnServerDiscovered");
		_innerProcess.MoveNext (Command.ConnectGame);
		ensureCorrectScene ();

		// stop listening for broadcasts
		if (_discoveryClient != null && !Flags.GAME_SERVER_IS_LOCALHOST) {
        	_discoveryClient.StopBroadcast();
        }

		if (Flags.GAME_SERVER_IS_LOCALHOST) {
			_networkLobbyManager.networkAddress = "localhost";
		} else {
			_networkLobbyManager.networkAddress = address;
		}

		_networkLobbyManager.networkPort = NetworkConstants.GAME_PORT;
		_networkLobbyManager.StartClient ();
	}

	private void OnUserConnectedToGame () {
        if (DEBUG) Debug.Log ("OnUserConnectedToGame");
		_innerProcess.MoveNext (Command.JoinedGame);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		ensureCorrectScene ();
	}

	private void OnUserDisconnectedToGame () {
        if (DEBUG) Debug.Log ("OnUserDisconnectedToGame");
		// we don't have a state change for this... yet
		Screen.sleepTimeout = _defaultSleepTimeout;
	}

	private void OnServerGameReady () {
        if (DEBUG) Debug.Log ("OnServerGameReady");
		_innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	//call at end of GameManager (client)
	public void OnGameLoaded () {
        if (DEBUG) Debug.Log ("Notifying server that we have finished loading game");
		_networkLobbyManager.SetGameLoaded ();
	}

	private void OnServerGameEnd () {
        if (DEBUG) Debug.Log ("OnServerGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	public void OnUserRequestPlaySandbox () {
        if (DEBUG) Debug.Log ("OnUserRequestPlaySandbox");
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
		_discoveryClient.StopBroadcast ();


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
