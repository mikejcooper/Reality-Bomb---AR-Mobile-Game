﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;
using ServerNetworking;
using UnityEngine.SceneManagement;
using GlobalNetworking;
using NetworkCompat;

public class ServerSceneManager : MonoBehaviour
{
    static bool DEBUG = false;

	private const int MIN_REQ_PLAYERS = 1;

	public enum MeshRetrievalState
	{
		Idle, Retrieving
	}

	public delegate void StateChange (ProcessState state);
	public delegate void OnAllPlayersLoaded();

	public event StateChange StateChangeEvent;
	public event OnAllPlayersLoaded OnAllPlayersLoadedEvent;

	public delegate void OnPlayerDisconnect();
	public event OnPlayerDisconnect OnPlayerDisconnectEvent;

	public MeshRetrievalState MeshRetrievalStatus = MeshRetrievalState.Idle;
	public NetworkLobbyPlayer LobbyPlayerPrefab;
	public GameObject GamePlayerPrefab;
	public GameMapObjects WorldMesh { get { return _meshTransferManager.ProduceGameObjects (); }}
	public int ConnectedPlayerCount { get; private set; }
	public int ReadyPlayerCount { get { return _networkLobbyManager.ReadyPlayerCount (); }}

	private GameLobbyManager _networkLobbyManager;
	private ColourPool _colourPool;
	private MeshDiscoveryServer _meshDiscoveryServer;
	private MeshTransferManager _meshTransferManager;
	private PlayerDataManager _playerDataManager;
	private Process _innerProcess;
	private MeshServerLifecycle.Process _meshTransferProcess;
	private string _currentScene = "Idle";

	private static ServerSceneManager _instance;

	public static ServerSceneManager Instance { get { return _instance; } }

	private string _meshServerAddress;
	private int _meshServerPort;

	public bool StartMuted;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
		_innerProcess = new Process();
		_meshTransferProcess = new MeshServerLifecycle.Process ();
	}

	void Start ()
	{
		DontDestroyOnLoad (gameObject);
		var ugly = UnityThreadHelper.Dispatcher;

		var discoveryServer = transform.gameObject.AddComponent<DiscoveryServer> ();

		// If clients will be looking at localhost anyway, don't
		// pollute the network with broadcasts
		if (!Flags.GAME_SERVER_IS_LOCALHOST) {
			discoveryServer.BeginBroadcast ();
		}

		_networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();

        _meshDiscoveryServer = new MeshDiscoveryServer ();
		_meshTransferManager = new MeshTransferManager ();
		_playerDataManager = new PlayerDataManager (_networkLobbyManager);

		_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		_networkLobbyManager.showLobbyGUI = false;
		_networkLobbyManager.minPlayers = MIN_REQ_PLAYERS;

		// seemingly weird neccesary hack. todo: add this to our compat implementation
		_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

		_colourPool = new ColourPool ();

		List<string> lobbyScenes = new List<string> ();
		lobbyScenes.Add ("Idle");
		lobbyScenes.Add ("Leaderboard");
		_networkLobbyManager.lobbyScenes = lobbyScenes;

		_networkLobbyManager.playScene = "Game";

		_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
		_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

		_networkLobbyManager.networkPort = NetworkConstants.GAME_PORT;

		_networkLobbyManager.StartServer ();

		// register listeners for when players connect / disconnect
		_networkLobbyManager.OnLobbyServerConnectedEvent += OnGamePlayerConnected;
		_networkLobbyManager.OnLobbyServerDisconnectedEvent += OnGamePlayerDisconnected;
		_networkLobbyManager.OnLobbyClientReadyToBeginEvent += OnGamePlayerReady;
		_networkLobbyManager.OnLobbyAllClientGamesLoadedEvent += OnGamePlayerAllLoaded;

		_meshDiscoveryServer.MeshServerDiscoveredEvent += OnMeshServerFound;

		_meshTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;

		OnServerRequestLoadNewMesh ();

		if (StartMuted) {
			AudioListener.volume = 0.0f;
		}
	}



	public ProcessState CurrentState () {
		return _innerProcess.CurrentState;
	}


	public void OnServerRequestLoadNewMesh () {
		if (DEBUG) Debug.Log ("OnRequestLoadNewMesh");
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.FindServer);

		if (Flags.MESH_SERVER_IS_LOCALHOST) {
			_meshDiscoveryServer.StartSearching();
		}


		// set all clients to not-ready
		_networkLobbyManager.SetAllClientsNotReady ();
		OnStateUpdate ();

		if (!Flags.MESH_SERVER_IS_LOCALHOST) {
			OnMeshServerFound ("localhost", 3111);
		}
	}


	private void OnMeshServerFound (string address, int port) {
        if (DEBUG) Debug.Log (string.Format("OnMeshServerFound, address: {0}", address));
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.Download);

		if (Flags.MESH_SERVER_IS_LOCALHOST) {
			_meshDiscoveryServer.StopSearching ();
		}

		_meshServerAddress = address;
		_meshServerPort = port;

		_meshTransferManager.FetchData(address, port);
		OnStateUpdate ();
	}

	private void OnMeshDataReceived () {
        if (DEBUG) Debug.Log ("OnMeshDataReceived");
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.DownloadFinished);

		// now that we've gone and fetched the mesh, updating the websocket
		// server's served version, it's ok to tell clients to get the mesh
		_networkLobbyManager.AllClientsGetMesh(_meshServerAddress, _meshServerPort);

		_innerProcess.MoveNext (Command.MeshReceived);
		OnStateUpdate ();
	}


	private void OnGamePlayerConnected (UnityEngine.Networking.NetworkConnection conn)
	{
        if (DEBUG) Debug.Log ("OnPlayerConnected");
		ConnectedPlayerCount++;


		int hue = _colourPool.getColour ();
		Debug.Log (string.Format ("added player with hue: {0}", hue));
		_playerDataManager.AddPlayer (conn.connectionId, hue);

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		OnStateUpdate ();

		if (_meshTransferProcess.CurrentState.Equals(MeshServerLifecycle.ProcessState.HasMesh)) {
			_networkLobbyManager.ClientGetMesh (_meshServerAddress, _meshServerPort, conn.connectionId);
		}

		_networkLobbyManager.SendPlayerID (conn.connectionId);
		_networkLobbyManager.SendPlayerData (JsonUtility.ToJson (_playerDataManager.list), conn.connectionId);
	}

	private void OnGamePlayerReady () {
		OnStateUpdate ();
	}


	private void OnGamePlayerAllLoaded () {
		if (OnAllPlayersLoadedEvent != null) {
            OnAllPlayersLoadedEvent();
		}
		OnStateUpdate ();
	}

	private void OnGamePlayerDisconnected (UnityEngine.Networking.NetworkConnection conn)
	{
        if (DEBUG) Debug.Log ("OnPlayerDisconnected");
		ConnectedPlayerCount--;

		_colourPool.releaseColour (_playerDataManager.GetPlayerById (conn.connectionId).colour);

		_playerDataManager.RemovePlayer (conn.connectionId);

		if (OnPlayerDisconnectEvent != null) {
			OnPlayerDisconnectEvent();
		}

		if (CurrentState().Equals(ServerLifecycle.ProcessState.CountingDown)) {
			_networkLobbyManager.AllClientsCancelGameCountdown ("not enough players");
			CancelInvoke ();
		}

		if (ConnectedPlayerCount < MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		OnStateUpdate ();
	}

	public void OnServerRequestCancelGameStart () {
		_innerProcess.MoveNext (Command.CountdownCancel);
		_networkLobbyManager.AllClientsCancelGameCountdown ("server cancelled");
		CancelInvoke ();
		OnStateUpdate ();
	}

	public void OnServerRequestGameStart (int delay) {
        if (DEBUG) Debug.Log ("OnGameIsReady");
		if (_networkLobbyManager.IsReadyToBegin ()) {
			// begin countdown
			_innerProcess.MoveNext (Command.CountdownStart);
			OnStateUpdate ();
			if (delay > 0) {
				_networkLobbyManager.AllClientsStartGameCountdown (delay);
				Invoke("BeginGame", delay);
			} else {
				BeginGame ();
			}
		} else {
			Debug.LogError ("Not all clients are ready. This means they haven't all loaded a mesh. try clicking 'load mesh' to force a reload");
		}
	}

	private void BeginGame () {
		_innerProcess.MoveNext (Command.GameStart);
		OnStateUpdate ();
	}

	public void UpdatePlayerGameData (int serverId, int carsLeft, float lifetime) {
		_playerDataManager.UpdatePlayerGameData (serverId, carsLeft, lifetime);
	}

	public PlayerDataManager.PlayerData[] GetPlayerData () {
		return _playerDataManager.list.players;
	}

	public PlayerDataManager.PlayerData GetPlayerDataById (int serverId) {
		return _playerDataManager.GetPlayerById (serverId);
	}

	public void OnServerRequestGameEnd () {
        if (DEBUG) Debug.Log ("OnGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		OnStateUpdate ();
//		 todo call correct things at game end
	}

	//Fade out current scene when switching scenes
	IEnumerator ChangeToGameScene() {
		float fadeTime = GameObject.Find ("Fade").GetComponent<SceneFade> ().BeginFade (1);
		yield return new WaitForSeconds (fadeTime);
		_currentScene = "Game"; 
		_playerDataManager.ResetAllGameData ();
		_networkLobbyManager.ServerChangeScene ("Game");
	}

	private void OnStateUpdate ()
	{
		switch (_innerProcess.CurrentState) {
		case ProcessState.AwaitingData:
		case ProcessState.AwaitingMesh:
		case ProcessState.AwaitingPlayers:
		case ProcessState.PreparingGame:
		case ProcessState.CountingDown:
			if (_playerDataManager.HasGameData()) {
				//				networkLobbyManager.ServerChangeScene ("Leaderboard");
				if (_currentScene != "Leaderboard") {
					_networkLobbyManager.ServerChangeScene("Leaderboard");
					_currentScene = "Leaderboard"; // put this in some scene load callback
					foreach (var lobbyPlayer in _networkLobbyManager.lobbySlots) {

						if (lobbyPlayer == null)
							continue;

						lobbyPlayer.GetComponent<NetworkCompat.NetworkLobbyPlayer> ().readyToBegin = true;

						// tell every player that this player is ready
						var outMsg = new LobbyReadyToBeginMessage ();
						outMsg.slotId = lobbyPlayer.slot;
						outMsg.readyState = true;
						UnityEngine.Networking.NetworkServer.SendToReady (null, UnityEngine.Networking.MsgType.LobbyReadyToBegin, outMsg);
					}
				}
			} else {
				if (_currentScene != "Idle") {
					// put this in some scene load callback
					_currentScene = "Idle";  
					SceneManager.LoadScene ("Idle");
				}
			}
			break;
		case ProcessState.PlayingGame:
			if (_currentScene != "Game") {
				// put this in some scene load callback
				StartCoroutine(ChangeToGameScene());
			}
			break;
		}

		if (StateChangeEvent != null)
			StateChangeEvent (_innerProcess.CurrentState);	
	}

}