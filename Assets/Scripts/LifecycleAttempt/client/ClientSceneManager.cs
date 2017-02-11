using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientLifecycle;
using ClientNetworking;
using UnityEngine.SceneManagement;

public class ClientSceneManager : MonoBehaviour
{

	private DiscoveryClient discoveryClient;
	private CommunicationClient communicationClient;

	private Process innerProcess;

	void Start ()
	{
		// init
		DontDestroyOnLoad (transform.gameObject);
		innerProcess = new Process ();	
		discoveryClient = transform.gameObject.AddComponent<DiscoveryClient> ();
		communicationClient = new CommunicationClient();

		// register listeners for when players connect / disconnect
		communicationClient.serverConnectedEvent += new CommunicationClient.ServerConnectedCallback (onUserConnectedToGame);
		communicationClient.serverDisconnectedEvent += new CommunicationClient.ServerDisconnectedCallback(onUserLeaveGame);

		discoveryClient.serverDiscoveryEvent += new DiscoveryClient.ServerDiscoveredCallback(onServerDiscovered);
	}

	public void onUserJoinGame () {
		Debug.Log ("onUserJoinGame");
		innerProcess.MoveNext (Command.JoinGame);
		ensureCorrectScene ();

		// begin listening
		discoveryClient.ListenForServers ();

	}

	public void onServerDiscovered (string address) {
		Debug.Log ("onServerDiscovered");
		innerProcess.MoveNext (Command.ConnectGame);
		ensureCorrectScene ();

		// attempt to connect to server
		communicationClient.ConnectToAddress (address);


		// stop listening for broadcasts
		if (discoveryClient != null) {
			discoveryClient.StopBroadcast ();
		}
	}

	public void onUserConnectedToGame () {
		Debug.Log ("onUserConnectedToGame");
		innerProcess.MoveNext (Command.JoinedGame);
		ensureCorrectScene ();
	}

	public void onServerGameReady () {
		Debug.Log ("onServerGameReady");
		innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	public void onServerGameEnd () {
		Debug.Log ("onServerGameEnd");
		innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	public void onUserPlayMinigame () {
		Debug.Log ("onUserPlayMinigame");
		innerProcess.MoveNext (Command.PlayMinigame);
		ensureCorrectScene ();
	}

	public void onUserLeaveGame () {
		Debug.Log ("onUserLeaveGame");
		innerProcess.MoveNext (Command.LeaveGame);
		ensureCorrectScene ();
	}


	private void ensureCorrectScene ()
	{
		switch (innerProcess.CurrentState) {
		case ProcessState.Idle:
		case ProcessState.Searching:
		case ProcessState.Connecting:
			SceneManager.LoadScene ("Idle");
			break;
		case ProcessState.MiniGame:
			SceneManager.LoadScene ("MiniGame");
			break;
		case ProcessState.PlayingGame:
			SceneManager.LoadScene ("Game");
			break;
		case ProcessState.Leaderboard:
			SceneManager.LoadScene ("Leaderboard");
			break;
		}
	}

}
