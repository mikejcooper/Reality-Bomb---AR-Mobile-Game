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

	private const int MIN_REQ_PLAYERS = 1;

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

	private GameLobbyManager _networkLobbyManager;
	private MeshDiscoveryServer _meshDiscoveryServer;
	private DataTransferManager _dataTransferManager;
	private Process _innerProcess;
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
    }

	void Start ()
	{
		DontDestroyOnLoad (gameObject);
		var ugly = UnityThreadHelper.Dispatcher;
		
		transform.gameObject.AddComponent<DiscoveryServer> ();
		_networkLobbyManager = transform.gameObject.AddComponent<GameLobbyManager> ();
		_meshDiscoveryServer = new MeshDiscoveryServer ();
		_dataTransferManager = new DataTransferManager ();

		_networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		_networkLobbyManager.showLobbyGUI = false;

		// seemingly weird neccesary hack. todo: add this to our compat implementation
		_networkLobbyManager.lobbySlots = new NetworkLobbyPlayer[_networkLobbyManager.maxPlayers];

		List<string> lobbyScenes = new List<string> ();
		lobbyScenes.Add ("Idle");
		lobbyScenes.Add ("Leaderboard");
		_networkLobbyManager.lobbyScenes = lobbyScenes;

		_networkLobbyManager.playScene = "Game";

		_networkLobbyManager.lobbyPlayerPrefab = LobbyPlayerPrefab;
		_networkLobbyManager.gamePlayerPrefab = GamePlayerPrefab;

		// don't think we need this
//		_networkLobbyManager.networkAddress = "localhost";
		_networkLobbyManager.networkPort = NetworkConstants.GAME_PORT;

		_networkLobbyManager.StartServer ();

		// register listeners for when players connect / disconnect
		_networkLobbyManager.OnLobbyServerConnectedEvent += OnPlayerConnected;
		_networkLobbyManager.OnLobbyServerDisconnectedEvent += OnPlayerDisconnected;
	

		_meshDiscoveryServer.MeshServerDiscoveredEvent += OnMeshServerFound;

		_dataTransferManager.OnMeshDataReceivedEvent += OnMeshDataReceived;
        _dataTransferManager.OnMeshDataProcessedEvent += OnMeshDataProcessed;

        SceneManager.sceneLoaded += OnSceneLoaded;
		// development
		OnServerRequestLoadNewMesh ();
	}
		
	public int ConnectedPlayerCount { get; private set; }

	public ProcessState CurrentState () {
		return _innerProcess.CurrentState;
	}


	public void OnServerRequestLoadNewMesh () {
		DebugConsole.Log ("OnRequestLoadNewMesh");
		Debug.Log ("OnRequestLoadNewMesh not implemented");
        _meshDiscoveryServer.StartSearching();
        //OnMeshDataReceived();
    }

	private void OnMeshServerFound (string address, int port) {
		DebugConsole.Log ("OnMeshServerFound");
        _meshDiscoveryServer.StopSearching();

        //Save for later use
        _meshServerAddress = address;
        _meshServerPort = port;
        //
        // now we ask some class to get the mesh data, with a callback when it's done
        _dataTransferManager.fetchData(address, port);

        //Tell clients to get their meshes from this address and port
        SocketMessage msg = new SocketMessage();
        msg.address = address;
        msg.port = port;
        UnityEngine.Networking.NetworkServer.SendToAll(928, msg);
        //Need to wait until all clients have downloaded before we change scenes really
    }

    private void OnMeshDataReceived () {
		DebugConsole.Log ("OnMeshDataReceived");

		_innerProcess.MoveNext (Command.MeshReceived);
		ensureCorrectScene ();
	}

    private void OnMeshDataProcessed()
    {
        DebugConsole.Log("OnMeshDataProcessed");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _dataTransferManager.produceMeshObject();
        }
    }

    private void OnPlayerConnected (UnityEngine.Networking.NetworkConnection conn)
	{
		DebugConsole.Log ("OnPlayerConnected");
		ConnectedPlayerCount++;

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		ensureCorrectScene ();

        //		// for the case that we have data already
        //		if (dataTransferManager.meshData != null) {
        //			// send this new client the meshd data
        //			DebugConsole.Log ("sending mesh to new client");
        //			UnityEngine.Networking.NetworkSystem.StringMessage outMsg = new UnityEngine.Networking.NetworkSystem.StringMessage (dataTransferManager.meshData);
        //			UnityEngine.Networking.NetworkServer.SendByChannelToAll(928, outMsg, 1);
        //		} else {
        //			DebugConsole.Log("not sending mesh to new client");
        //		}
        SocketMessage msg = new SocketMessage();
        msg.address = _meshServerAddress;
        msg.port = _meshServerPort;
        UnityEngine.Networking.NetworkServer.SendToClient(conn.connectionId, 928, msg);
    }

	private void OnPlayerDisconnected ()
	{
		DebugConsole.Log ("OnPlayerDisconnected");
		ConnectedPlayerCount--;

		if (ConnectedPlayerCount < MIN_REQ_PLAYERS) {
			_innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		ensureCorrectScene ();
	}

	public void OnServerRequestGameReady () {
		DebugConsole.Log ("OnGameIsReady");
		_innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	public void OnServerRequestGameEnd () {
		DebugConsole.Log ("OnGameEnd");
		_innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
		// todo call correct things at game end
	}

	private void ensureCorrectScene ()
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
					
						lobbyPlayer.GetComponent<UnityEngine.Networking.NetworkLobbyPlayer> ().readyToBegin = true;

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
			}
			break;
		}

		if (StateChangeEvent != null)
			StateChangeEvent (_innerProcess.CurrentState);
	}

}