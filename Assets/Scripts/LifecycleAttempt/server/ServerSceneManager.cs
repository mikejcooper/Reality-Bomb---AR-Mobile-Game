using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;
using ServerNetworking;
using UnityEngine.SceneManagement;

public class ServerSceneManager : MonoBehaviour
{
	

	private const int MIN_REQ_PLAYERS = 2;

	private CommunicationServer communicationServer;
	private DiscoveryServer discoveryServer;
	private Process innerProcess;
	private Mesh currentMesh;
	private int currentPlayerCount = 0;


	void Start ()
	{
		// init
		DontDestroyOnLoad (transform.gameObject);
		innerProcess = new Process ();
		discoveryServer = transform.gameObject.AddComponent<DiscoveryServer> ();
		communicationServer = new CommunicationServer ();

		// register listeners for when players connect / disconnect
		communicationServer.clientConnectedCallback += new CommunicationServer.ClientConnectedCallback (onPlayerConnected);
		communicationServer.clientDisconnectedCallback += new CommunicationServer.ClientDisconnectedCallback (onPlayerDisconnected);
	}

	public void onMeshReceived (Mesh mesh)
	{
		Debug.Log ("onMeshReceived");
		currentMesh = mesh;
		innerProcess.MoveNext (Command.MeshReceived);
		ensureCorrectScene ();
	}

	public void onPlayerConnected ()
	{
		Debug.Log ("onPlayerConnected");
		currentPlayerCount++;

		if (currentPlayerCount >= MIN_REQ_PLAYERS) {
			innerProcess.MoveNext (Command.EnoughPlayersJoined);
		}
		ensureCorrectScene ();
	}

	public void onPlayerDisconnected ()
	{
		Debug.Log ("onPlayerDisconnected");
		currentPlayerCount--;

		if (currentPlayerCount < MIN_REQ_PLAYERS) {
			innerProcess.MoveNext (Command.TooFewPlayersRemaining);
		}
		ensureCorrectScene ();
	}

	public void onGameReady () {
		Debug.Log ("onGameReady");
		innerProcess.MoveNext (Command.GameReady);
		ensureCorrectScene ();
	}

	public void onGameEnd () {
		Debug.Log ("onGameEnd");
		innerProcess.MoveNext (Command.GameEnd);
		ensureCorrectScene ();
	}

	private void ensureCorrectScene ()
	{
		switch (innerProcess.CurrentState) {
		case ProcessState.AwaitingData:
		case ProcessState.AwaitingMesh:
		case ProcessState.AwaitingPlayers:
		case ProcessState.PreparingGame:
			SceneManager.LoadScene ("Idle");
			break;
		case ProcessState.PlayingGame:
			SceneManager.LoadScene ("Game");
			break;
		}
	}

}
