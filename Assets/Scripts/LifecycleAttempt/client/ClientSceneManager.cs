﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientLifecycle;
using ClientNetworking;
using UnityEngine.SceneManagement;

public class ClientSceneManager : MonoBehaviour
{

	public GameObject capsulePrefab;
	public NetworkCompat.NetworkLobbyPlayer lobbyPlayerPrefab;
	public GameObject gamePlayerPrefab;

	public GameResults lastGameResults;

	private DiscoveryClient discoveryClient;
	private PTBGameLobbyManager networkLobbyManager;

	private Process innerProcess;
	private string CurrentScene = "Idle";

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
		// init
		DontDestroyOnLoad (gameObject);
		innerProcess = new Process ();	
		discoveryClient = transform.gameObject.AddComponent<DiscoveryClient> ();
		networkLobbyManager = transform.gameObject.AddComponent<PTBGameLobbyManager> ();

		networkLobbyManager.logLevel = UnityEngine.Networking.LogFilter.FilterLevel.Debug;
		networkLobbyManager.showLobbyGUI = false;

		networkLobbyManager.lobbySlots = new NetworkCompat.NetworkLobbyPlayer[networkLobbyManager.maxPlayers];

		List<string> lobbyScenes = new List<string> ();
		lobbyScenes.Add ("Idle");
		lobbyScenes.Add ("Leaderboard");
		networkLobbyManager.lobbyScenes = lobbyScenes;

		networkLobbyManager.playScene = "Game";

		networkLobbyManager.lobbyPlayerPrefab = lobbyPlayerPrefab;
		networkLobbyManager.gamePlayerPrefab = gamePlayerPrefab;

		// register listeners for when players connect / disconnect
		networkLobbyManager.OnLobbyClientConnectEvent += onUserConnectedToGame;
		networkLobbyManager.OnLobbyClientDisconnectEvent += onUserLeaveGame;

		discoveryClient.serverDiscoveryEvent += new DiscoveryClient.ServerDiscoveredCallback(onServerDiscovered);

		SceneManager.sceneLoaded += new UnityEngine.Events.UnityAction<Scene, LoadSceneMode> (OnSceneLoaded);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.name == "Game") {
			CurrentScene = "Game";
			onServerGameReady ();
			// notify server we've loaded
//			communicationClient.NotifySceneLoaded(scene.name, capsulePrefab);
		} else if (scene.name == "Leaderboard") {
			CurrentScene = "Leaderboard";
			onServerGameEnd ();
		}
	}

	public void onUserJoinGame () {
		DebugConsole.Log ("onUserJoinGame");
		innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		// begin listening
		discoveryClient.ListenForServers ();

	}

	public void onServerDiscovered (string address) {
		DebugConsole.Log ("onServerDiscovered");
		innerProcess.MoveNext (Command.ConnectGame);
		ensureCorrectScene ();

		// stop listening for broadcasts
		if (discoveryClient != null) {
			discoveryClient.StopBroadcast ();
		}

//		networkLobbyManager.connectionConfig.AddChannel (UnityEngine.Networking.QosType.Reliable);
//		networkLobbyManager.connectionConfig.AddChannel (UnityEngine.Networking.QosType.ReliableFragmented);

		networkLobbyManager.networkAddress = "localhost";
		networkLobbyManager.networkPort = 7777;
		networkLobbyManager.StartClient ();
	}

	public void onUserConnectedToGame () {
		DebugConsole.Log ("onUserConnectedToGame");
		innerProcess.MoveNext (Command.JoinedGame);
		ensureCorrectScene ();
	}

	public void onServerGameReady () {
		DebugConsole.Log ("onServerGameReady");
		innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	public void onServerGameEnd () {
		DebugConsole.Log ("onServerGameEnd");
		innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	public void onUserPlayMinigame () {
		DebugConsole.Log ("onUserPlayMinigame");
		innerProcess.MoveNext (Command.PlayMinigame);
		ensureCorrectScene ();
	}

	public void onUserLeaveGame () {
		DebugConsole.Log ("onUserLeaveGame");
		innerProcess.MoveNext (Command.LeaveGame);

		networkLobbyManager.StopClient ();


		ensureCorrectScene ();
	}


	private void ensureCorrectScene ()
	{
		switch (innerProcess.CurrentState) {
		case ProcessState.Idle:
		case ProcessState.Searching:
		case ProcessState.Connecting:
			if (CurrentScene != "Idle") {
				CurrentScene = "Idle";
				SceneManager.LoadScene ("Idle");
			}
			break;
		case ProcessState.MiniGame:
			if (CurrentScene != "MiniGame") {
				CurrentScene = "MiniGame";
				SceneManager.LoadScene ("MiniGame");
			}
			break;
		case ProcessState.PlayingGame:
			// this is managed for us by LobbyNetworkManager
//			if (CurrentScene != "Game") {
//				CurrentScene = "Game";
//				SceneManager.LoadScene ("Game");
//			}
			break;
		case ProcessState.Leaderboard:
			// this is managed for us by LobbyNetworkManager
//			if (CurrentScene != "Leaderboard") {
//				CurrentScene = "Leaderboard";
//				SceneManager.LoadScene ("Leaderboard");
//			}
			break;
		}
	}

}
