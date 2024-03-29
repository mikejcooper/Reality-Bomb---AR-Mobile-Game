﻿using System.Collections;
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
	private const int MAX_DEVICES = 16;

	public delegate void OnCountDownTimeUpdateCallback (int remainingTime);
	public delegate void OnCountDownCanceledCallback (string reason);

	public OnCountDownTimeUpdateCallback OnCountDownTimeUpdateEvent = delegate {};
	public OnCountDownCanceledCallback OnCountDownCanceledEvent = delegate {};
	public NetworkLobbyPlayer LobbyPlayerPrefab;
	public GameObject GamePlayerPrefab;
	public GameMapObjects WorldMesh { get { return _meshTransferManager.ProduceGameObjects (); }}

	private DiscoveryClient _discoveryClient;
	private GameLobbyManager _networkLobbyManager;
    private MeshTransferManager _meshTransferManager;
	private Process _innerProcess;
	private string _currentScene = "Idle";
	private static ClientSceneManager _instance;
	private int _defaultSleepTimeout;
	private Coroutine _countdownCoroutine;

	public string ClientNickName;
	public int ClientVehicleId;

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

	void ConfigNetLobby() {
		if (_networkLobbyManager != null) {
			_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Info;
			_networkLobbyManager.showLobbyGUI = false;

			_networkLobbyManager.maxPlayers = MAX_DEVICES;
			_networkLobbyManager.maxConnections = MAX_DEVICES;
			_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

			List<string> lobbyScenes = new List<string> ();
			lobbyScenes.Add ("Idle");
			lobbyScenes.Add ("Leaderboard");
			_networkLobbyManager.lobbyScenes = lobbyScenes;

			_networkLobbyManager.playScene = "Game";

			_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
			_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

			_networkLobbyManager.customConfig = true;
			var config = _networkLobbyManager.connectionConfig;
			config.NetworkDropThreshold = 80;
			config.ConnectTimeout = 10000;
			config.DisconnectTimeout = 10000;
			config.PingTimeout = 2000;
			_networkLobbyManager.channels.Clear ();
			_networkLobbyManager.channels.Add (UnityEngine.Networking.QosType.ReliableFragmented);
			_networkLobbyManager.channels.Add (UnityEngine.Networking.QosType.ReliableFragmented);
		}
	}

	GameLobbyManager CreateNetLobby () {
		if (transform.gameObject.GetComponent<GameLobbyManager> () != null) {
			Debug.LogError ("trying to create a game lobby manager when one already exists!");
			return transform.gameObject.GetComponent<GameLobbyManager> ();
		}
		var networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();
		// register listeners for when players connect / disconnect
		networkLobbyManager.OnLobbyClientConnectedEvent += OnUserConnectedToGame;
		networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserDisconnectedToGame;
		networkLobbyManager.OnLobbyClientDisconnectedEvent += OnUserRequestLeaveGame;
		networkLobbyManager.OnMeshClearToDownloadEvent += _meshTransferManager.FetchData;
		networkLobbyManager.OnStartGameCountdownEvent += OnStartGameCountdown;
		networkLobbyManager.OnCancelGameCountdownEvent += OnCancelGameCountdown;

		return networkLobbyManager;
	}

	void Start ()
	{
		DontDestroyOnLoad (gameObject);
		var ugly = UnityThreadHelper.Dispatcher;


		_meshTransferManager = new MeshTransferManager();

		_defaultSleepTimeout = Screen.sleepTimeout;




        //Listener for when the we have finished downloading the mesh
		_meshTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;


		SceneManager.sceneLoaded += OnSceneLoaded;

		if (StartMuted) {
			AudioListener.volume = 0.0f;
		}
	}
		
    private void OnMeshDataReceived()
    {
		if (DEBUG) Debug.Log("OnMeshDataReceived");

		if (_networkLobbyManager != null) {
			_networkLobbyManager.SetReady ();
		}

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

	public void OnUserRequestFindGame (string nickname, Garage.Vehicle vehicle) {
		if (DEBUG) Debug.Log ("OnUserRequestFindGame");

		Debug.Log (string.Format ("user requested nickname: {0} and vehicle id: {1}", nickname, vehicle.Id));
		ClientNickName = nickname;
		ClientVehicleId = vehicle.Id;
		_innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		if (Flags.GAME_SERVER_IS_LOCALHOST) {
			OnServerDiscovered (":ffff::localhost");
		} else {
			// begin listening
			_discoveryClient = transform.gameObject.AddComponent<DiscoveryClient> ();
			_discoveryClient.serverDiscoveryEvent += OnServerDiscovered;
			_discoveryClient.ListenForServers ();
		}

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
			_countdownCoroutine = StartCoroutine (Countdown (newRemaining, delayTime));
		} else {
			OnCountDownTimeUpdateEvent (0);
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
		if (_discoveryClient != null && _discoveryClient.running && !Flags.GAME_SERVER_IS_LOCALHOST) {
			_discoveryClient.serverDiscoveryEvent -= OnServerDiscovered;
        	_discoveryClient.StopBroadcast();
			Destroy (_discoveryClient);
        }

		var ipv4 = address.Substring (address.LastIndexOf (":")+1, address.Length - (address.LastIndexOf (":") + 1));
		Debug.Log (string.Format ("ipv6: {0}, ipv4: {1}", address, ipv4));


		if (_networkLobbyManager == null) {
			_networkLobbyManager = CreateNetLobby ();
		}

		if (Flags.GAME_SERVER_IS_LOCALHOST) {
			_networkLobbyManager.networkAddress = Network.player.ipAddress;
		} else {
			_networkLobbyManager.networkAddress = ipv4;
		}

		Debug.Log (string.Format ("found server at {0}", _networkLobbyManager.networkAddress));

		ConfigNetLobby ();


		// we have to delay, because apparently the teardown of discovery
		// impacts the next part. 2 seconds seems long enough...
		Invoke ("DelayedConnect", 2);
	}

	private void DelayedConnect () {
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
		
	public void OnUserRequestLeaveGame () {
		Debug.Log ("OnUserRequestLeaveGame");
		_innerProcess.MoveNext (Command.LeaveGame);

		CancelInvoke ("DelayedConnect");

		if (_networkLobbyManager != null) {
			_networkLobbyManager.StopClient ();
			Destroy (_networkLobbyManager);
		}

		if (_discoveryClient != null) {
			_discoveryClient.serverDiscoveryEvent -= OnServerDiscovered;
			_discoveryClient.StopBroadcast();
			Destroy (_discoveryClient);
		}

		ensureCorrectScene ();
	}

	public NetworkCompat.NetworkLobbyPlayer GetOwnLobbyPlayer () {
		foreach (var lobbyPlayer in GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ()) {
			if (lobbyPlayer.isLocalPlayer) {
				return lobbyPlayer;
			}
		}
		return null;
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
