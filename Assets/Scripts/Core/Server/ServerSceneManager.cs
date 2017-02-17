using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;
using ServerNetworking;
using UnityEngine.SceneManagement;
using GlobalNetworking;
using NetworkCompat;

public class ServerSceneManager : MonoBehaviour
{

	private const int MIN_REQ_PLAYERS = 2;

	public enum MeshRetrievalState
	{
		Idle, Retrieving
	}

	public delegate void StateChange (ProcessState state);
	public event StateChange StateChangeEvent;

	public MeshRetrievalState MeshRetrievalStatus = MeshRetrievalState.Idle;
	public NetworkLobbyPlayer LobbyPlayerPrefab;
	public GameObject GamePlayerPrefab;
	public GameResults LastGameResults;
	public GameObject WorldMesh { get { return _meshTransferManager.ProduceGameObject (); }}
	public int ConnectedPlayerCount { get; private set; }
	public int ReadyPlayerCount { get { return _networkLobbyManager.ReadyPlayerCount (); }}

	private GameLobbyManager _networkLobbyManager;
	private MeshDiscoveryServer _meshDiscoveryServer;
	private MeshTransferManager _meshTransferManager;
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
		_networkLobbyManager.OnLobbyServerConnectedEvent += OnPlayerConnected;
		_networkLobbyManager.OnLobbyServerDisconnectedEvent += OnPlayerDisconnected;
		_networkLobbyManager.OnLobbyClientReadyToBeginEvent += OnPlayerReady;

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
		DebugConsole.Log ("OnMeshServerFound");
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


	private void OnPlayerConnected (UnityEngine.Networking.NetworkConnection conn)
	{
		DebugConsole.Log ("OnPlayerConnected");
		ConnectedPlayerCount++;

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		OnStateUpdate ();

		if (_meshTransferProcess.CurrentState == MeshServerLifecycle.ProcessState.HasMesh) {
			_networkLobbyManager.ClientGetMesh (_meshServerAddress, _meshServerPort, conn.connectionId);
		}
    }

	private void OnPlayerReady () {
		OnStateUpdate ();
	}

	private void OnPlayerDisconnected ()
	{
		DebugConsole.Log ("OnPlayerDisconnected");
		ConnectedPlayerCount--;

		if (ConnectedPlayerCount < MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		OnStateUpdate ();
	}

	public void OnServerRequestGameReady () {
		DebugConsole.Log ("OnGameIsReady");
		_innerProcess.MoveNext (Command.GameReady);
		OnStateUpdate ();
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
			if (LastGameResults != null) {
//				networkLobbyManager.ServerChangeScene ("Leaderboard");
				if (_currentScene != "Leaderboard") {
					Debug.LogError("sent a scene change: Leaderboard");
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
				_networkLobbyManager.CheckReadyToBegin ();
				_networkLobbyManager.ServerChangeScene("Game");
			}
			break;
		}

		if (StateChangeEvent != null)
			StateChangeEvent (_innerProcess.CurrentState);
	}

}