using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;
using ServerNetworking;
using UnityEngine.SceneManagement;

public class ServerSceneManager : MonoBehaviour
{

	public enum MeshRetrievalState
	{
		Idle, Retrieving
	}

	public static ServerSceneManager instance;

	public MeshRetrievalState meshRetrievalState = MeshRetrievalState.Idle;

	public UnityEngine.Networking.NetworkLobbyPlayer lobbyPlayerPrefab;
	public GameObject gamePlayerPrefab;

	public GameResults lastGameResults;

	public delegate void StateChangeCallback(ProcessState state);
	public event StateChangeCallback stateChangeEvent;

	private const int MIN_REQ_PLAYERS = 2;

	private DiscoveryServer discoveryServer;
	private PTBGameLobbyManager networkLobbyManager;
	private MeshDiscoveryServer meshDiscoveryServer;
	private DataTransferManager dataTransferManager;
	private Process innerProcess;

	private string currentScene = "Idle";
	private int LoadedPlayerCount = 0;

	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this){
			Destroy(gameObject);
		}
		DontDestroyOnLoad (gameObject);

		Init ();
	}

	void Init ()
	{
		var ugly = UnityThreadHelper.Dispatcher;

		// init
		innerProcess = new Process ();
		discoveryServer = transform.gameObject.AddComponent<DiscoveryServer> ();
		networkLobbyManager = transform.gameObject.AddComponent<PTBGameLobbyManager> ();
		meshDiscoveryServer = new MeshDiscoveryServer ();
		dataTransferManager = new DataTransferManager ();

		networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		networkLobbyManager.showLobbyGUI = false;

//		networkLobbyManager.connectionConfig.AddChannel (UnityEngine.Networking.QosType.Reliable);
//		networkLobbyManager.connectionConfig.AddChannel (UnityEngine.Networking.QosType.ReliableFragmented);

		// register listeners for when players connect / disconnect
		networkLobbyManager.lobbySlots = new UnityEngine.Networking.NetworkLobbyPlayer[networkLobbyManager.maxPlayers];
		networkLobbyManager.lobbyScene = "Idle";

		networkLobbyManager.playScene = "Game";

		networkLobbyManager.lobbyPlayerPrefab = lobbyPlayerPrefab;
		networkLobbyManager.gamePlayerPrefab = gamePlayerPrefab;

		networkLobbyManager.networkAddress = "localhost";
		networkLobbyManager.networkPort = 7777;

		networkLobbyManager.StartServer ();


		networkLobbyManager.OnLobbyServerConnectEvent += onPlayerConnected;
		networkLobbyManager.OnLobbyServerDisconnectEvent += onPlayerDisconnected;
	

		meshDiscoveryServer.meshServerDiscoveredCallback += new MeshDiscoveryServer.MeshServerDiscoveredCallback (onMeshServerFound);

		dataTransferManager.onMeshDataReceivedEvent += onMeshDataReceived;

		loadNewMesh ();
	}

	public int ConnectedPlayerCount { get; private set; }

	public ProcessState CurrentState () { 
		return innerProcess.CurrentState;
	}


	public void loadNewMesh() {
		DebugConsole.Log ("loadNewMesh");
//		meshDiscoveryServer.StartSearching ();
		onMeshReceived();
	}

	public void onMeshServerFound (string address, int port) {
		DebugConsole.Log ("onMeshServerFound");
//		meshDiscoveryServer.StopSearching ();
//
//		// now we ask some class to get the mesh data, with a callback when it's done
//		dataTransferManager.fetchData(address, port);

	}

	public void onMeshDataReceived () {
		DebugConsole.Log ("onMeshDataReceived");
		// invalidate all clients

		// now send to all clients
		UnityEngine.Networking.NetworkSystem.StringMessage outMsg = new UnityEngine.Networking.NetworkSystem.StringMessage(dataTransferManager.meshData);
		UnityEngine.Networking.NetworkServer.SendToAll(3110, outMsg);
	}

	public void onMeshReceived ()
	{
		DebugConsole.Log ("onMeshReceived");
		innerProcess.MoveNext (Command.MeshReceived);
		ensureCorrectScene ();
	}

	public void onPlayerConnected (UnityEngine.Networking.NetworkConnection conn)
	{
		DebugConsole.Log ("onPlayerConnected");
		ConnectedPlayerCount++;

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			innerProcess.MoveNext (Command.EnoughPlayersJoined);
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
	}

	public void onPlayerDisconnected ()
	{
		DebugConsole.Log ("onPlayerDisconnected");
		ConnectedPlayerCount--;

		if (ConnectedPlayerCount < MIN_REQ_PLAYERS) {
			innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		ensureCorrectScene ();
	}

	public void onGameReady () {
		DebugConsole.Log ("onGameReady");
		innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	public void onGameEnd (GameResults gameResults) {
		lastGameResults = gameResults;
		DebugConsole.Log ("onGameEnd");
		innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
		// todo call correct things at game end
	}

	private void ensureCorrectScene ()
	{
		switch (innerProcess.CurrentState) {
		case ProcessState.AwaitingData:
		case ProcessState.AwaitingMesh:
		case ProcessState.AwaitingPlayers:
		case ProcessState.PreparingGame:
			if (lastGameResults != null) {
				networkLobbyManager.ServerChangeScene ("Leaderboard");
			} else {
				if (currentScene != "Idle") {
					currentScene = "Idle";
					SceneManager.LoadScene ("Idle");
				}
			}
			break;
		case ProcessState.PlayingGame:
			if (currentScene != "Game") {
				currentScene = "Game";
				LoadedPlayerCount = 0;

				networkLobbyManager.ServerChangeScene ("Game");
//				communicationServer.ChangeClientsScene ("Game");

//				SceneManager.LoadScene ("Pass_The_Bomb");
			}
			break;
		}

		if (stateChangeEvent != null)
		stateChangeEvent (innerProcess.CurrentState);
	}

}