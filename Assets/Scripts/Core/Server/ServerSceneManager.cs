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

	private const int MIN_REQ_PLAYERS = 1;

	public enum MeshRetrievalState
	{
		Idle, Retrieving
	}

	public delegate void StateChange (ProcessState state);
	public delegate void OnPlayerGameLoaded();

	public event StateChange StateChangeEvent;
	public event OnPlayerGameLoaded OnPlayerGameLoadedEvent;

	public MeshRetrievalState MeshRetrievalStatus = MeshRetrievalState.Idle;
	public NetworkLobbyPlayer LobbyPlayerPrefab;
	public GameObject GamePlayerPrefab;
	public GameObject WorldMesh { get { return _meshTransferManager.ProduceGameObject (); }}
	public int ConnectedPlayerCount { get; private set; }
	public int ReadyPlayerCount { get { return _networkLobbyManager.ReadyPlayerCount (); }}

	private GameLobbyManager _networkLobbyManager;
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

		transform.gameObject.AddComponent<DiscoveryServer> ();
		_networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();

		_meshDiscoveryServer = new MeshDiscoveryServer ();
		_meshTransferManager = new MeshTransferManager ();
		_playerDataManager = new PlayerDataManager (_networkLobbyManager);

		_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		_networkLobbyManager.showLobbyGUI = false;
		_networkLobbyManager.minPlayers = MIN_REQ_PLAYERS;

		// seemingly weird neccesary hack. todo: add this to our compat implementation
		_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

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
		_networkLobbyManager.OnLobbyClientGameLoadedEvent += OnGamePlayerGameLoaded;

		_meshDiscoveryServer.MeshServerDiscoveredEvent += OnMeshServerFound;

		_meshTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;

		OnServerRequestLoadNewMesh ();
	}



	public ProcessState CurrentState () {
		return _innerProcess.CurrentState;
	}


	public void OnServerRequestLoadNewMesh () {
		DebugConsole.Log ("OnRequestLoadNewMesh");
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.FindServer);
		_meshDiscoveryServer.StartSearching();

		// set all clients to not-ready
		_networkLobbyManager.SetAllClientsNotReady ();
		OnStateUpdate ();
	}

	private void OnMeshServerFound (string address, int port) {
		DebugConsole.Log (string.Format("OnMeshServerFound, address: {0}", address));
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.Download);

		_meshDiscoveryServer.StopSearching();

		_meshServerAddress = address;
		_meshServerPort = port;

		_meshTransferManager.FetchData(address, port);
		OnStateUpdate ();
	}

	private void OnMeshDataReceived () {
		DebugConsole.Log ("OnMeshDataReceived");
		_meshTransferProcess.MoveNext (MeshServerLifecycle.Command.DownloadFinished);

		// now that we've gone and fetched the mesh, updating the websocket
		// server's served version, it's ok to tell clients to get the mesh
		_networkLobbyManager.AllClientsGetMesh(_meshServerAddress, _meshServerPort);

		_innerProcess.MoveNext (Command.MeshReceived);
		OnStateUpdate ();
	}


	private void OnGamePlayerConnected (UnityEngine.Networking.NetworkConnection conn)
	{
		DebugConsole.Log ("OnPlayerConnected");
		ConnectedPlayerCount++;

		_playerDataManager.AddPlayer (conn.connectionId, NameGenerator.GenerateName ());

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		OnStateUpdate ();

		if (_meshTransferProcess.CurrentState == MeshServerLifecycle.ProcessState.HasMesh) {
			_networkLobbyManager.ClientGetMesh (_meshServerAddress, _meshServerPort, conn.connectionId);
		}

		_networkLobbyManager.SendPlayerID (conn.connectionId);
		_networkLobbyManager.SendPlayerData (JsonUtility.ToJson (_playerDataManager.list), conn.connectionId);
	}

	private void OnGamePlayerReady () {
		OnStateUpdate ();
	}


	private void OnGamePlayerGameLoaded () {
		if (OnPlayerGameLoadedEvent != null) {
			OnPlayerGameLoadedEvent ();
		}
		OnStateUpdate ();
	}

	private void OnGamePlayerDisconnected (UnityEngine.Networking.NetworkConnection conn)
	{
		DebugConsole.Log ("OnPlayerDisconnected");
		ConnectedPlayerCount--;

		_playerDataManager.RemovePlayer (conn.connectionId);

		if (ConnectedPlayerCount < MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		OnStateUpdate ();
	}

	public void OnServerRequestGameReady () {
		DebugConsole.Log ("OnGameIsReady");
		if (_networkLobbyManager.IsReadyToBegin ()) {
			_innerProcess.MoveNext (Command.GameReady);
			OnStateUpdate ();
		} else {
			Debug.LogError ("Not all clients are ready. This means they haven't all loaded a mesh. try clicking 'load mesh' to force a reload");
		}
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
		DebugConsole.Log ("OnGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		OnStateUpdate ();
		// todo call correct things at game end
	}

	private void OnStateUpdate ()
	{
		switch (_innerProcess.CurrentState) {
		case ProcessState.AwaitingData:
		case ProcessState.AwaitingMesh:
		case ProcessState.AwaitingPlayers:
		case ProcessState.PreparingGame:
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
				_currentScene = "Game"; 

				// this needs to be called before we change scene
				if (_networkLobbyManager.IsReadyToBegin ()) {
					_playerDataManager.ResetAllGameData ();
					_networkLobbyManager.ServerChangeScene ("Game");
				}
			}
			break;
		}

		if (StateChangeEvent != null)
			StateChangeEvent (_innerProcess.CurrentState);	
	}

}