using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;
using ServerNetworking;
using UnityEngine.SceneManagement;

public class ServerSceneManager : MonoBehaviour
{

	public UnityEngine.Networking.NetworkLobbyPlayer lobbyPlayerPrefab;
	public GameObject gamePlayerPrefab;


	public delegate void StateChangeCallback(ProcessState state);
	public event StateChangeCallback stateChangeEvent;

	private const int MIN_REQ_PLAYERS = 1;

//	private CommunicationServer communicationServer;
	private DiscoveryServer discoveryServer;
	private PTBGameLobbyManager networkLobbyManager;
//	private MeshDiscoveryServer meshDiscoveryServer;
	private Process innerProcess;
	private Mesh currentMesh;
	private string currentScene = "Idle";
	private int LoadedPlayerCount = 0;


	void Start ()
	{
		// init
		DontDestroyOnLoad (transform.gameObject);
		innerProcess = new Process ();
		discoveryServer = transform.gameObject.AddComponent<DiscoveryServer> ();
		networkLobbyManager = transform.gameObject.AddComponent<PTBGameLobbyManager> ();
//		communicationServer = new CommunicationServer ();
//		meshDiscoveryServer = new MeshDiscoveryServer ();

		// register listeners for when players connect / disconnect

		networkLobbyManager.lobbyScene = "Idle";

		networkLobbyManager.playScene = "Game";

		networkLobbyManager.lobbyPlayerPrefab = lobbyPlayerPrefab;
		networkLobbyManager.gamePlayerPrefab = gamePlayerPrefab;

		networkLobbyManager.OnLobbyServerConnectEvent += onPlayerConnected;
		networkLobbyManager.OnLobbyServerDisconnectEvent += onPlayerDisconnected;
	

//		meshDiscoveryServer.meshServerDiscoveredCallback += new MeshDiscoveryServer.MeshServerDiscoveredCallback (onMeshServerFound);

		onStartWaitingForData ();
	}

	public int ConnectedPlayerCount { get; private set; }

	public ProcessState CurrentState () { 
		return innerProcess.CurrentState;
	}


	public void onStartWaitingForData() {
//		meshDiscoveryServer.StartSearching ();
		// for now
		onMeshReceived (null);
	}

	public void onMeshServerFound (string address, int port) {
//		meshDiscoveryServer.StopSearching ();
		// for now
		onMeshReceived (null);
	}

	public void onMeshReceived (Mesh mesh)
	{
		DebugConsole.Log ("onMeshReceived");
		currentMesh = mesh;
		innerProcess.MoveNext (Command.MeshReceived);
		ensureCorrectScene ();
	}

	public void onPlayerConnected ()
	{
		DebugConsole.Log ("onPlayerConnected");
		ConnectedPlayerCount++;

		if (ConnectedPlayerCount >= MIN_REQ_PLAYERS) {
			innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		ensureCorrectScene ();
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

	public void onGameEnd () {
		DebugConsole.Log ("onGameEnd");
		innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
		onStartWaitingForData ();
	}

	private void ensureCorrectScene ()
	{
		switch (innerProcess.CurrentState) {
		case ProcessState.AwaitingData:
		case ProcessState.AwaitingMesh:
		case ProcessState.AwaitingPlayers:
		case ProcessState.PreparingGame:
			if (currentScene != "Idle") {
				currentScene = "Idle";
				SceneManager.LoadScene ("Idle");

			}
			break;
		case ProcessState.PlayingGame:
			if (currentScene != "Game") {
				currentScene = "Game";
				LoadedPlayerCount = 0;

//				communicationServer.ChangeClientsScene ("Game");
				SceneManager.LoadScene ("Game");
			}
			break;
		}
		stateChangeEvent (innerProcess.CurrentState);
	}

}
