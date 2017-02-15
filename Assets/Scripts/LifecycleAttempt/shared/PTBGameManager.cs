using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkBehaviour {

	public int bombPlayerConnectionId;

	private List<CarController> cars = new List<CarController>();

	private List<string> deathList = new List<string>();


	private static PTBGameManager _instance;

	public static PTBGameManager Instance { get { return _instance; } }


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
		if (!isServer) {
			ClientSceneManager.Instance.lastGameResults = new GameResults ();
		} else if (isServer) {
			ServerSceneManager.Instance.lastGameResults = new GameResults ();

			List<int> activeConnectionIds = new List<int>();
			for (int i=0; i<UnityEngine.Networking.NetworkServer.connections.Count; i++) {
				if (UnityEngine.Networking.NetworkServer.connections [i] != null &&
				    UnityEngine.Networking.NetworkServer.connections [i].isConnected) {
					activeConnectionIds.Add (UnityEngine.Networking.NetworkServer.connections [i].connectionId);
				}
			}
			int bombPlayerIndex = Random.Range(0, activeConnectionIds.Count);
			DebugConsole.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, activeConnectionIds.Count));
			bombPlayerConnectionId = activeConnectionIds [bombPlayerIndex];
			DebugConsole.Log ("=> bombPlayerConnectionId: " + bombPlayerConnectionId);
		}
	}


	[Server]
	private string getPlayerName (CarController car) {
		return string.Format ("player {0}", car.connectionToClient.connectionId);
	}

	[Server]
	public void AllDevicesKillPlayer (CarController car) {
		string playerName = getPlayerName (car);
		RpcKillPlayer (playerName);
		ServerKillPlayer (playerName);
		CheckForGameOver ();
	}

	[ClientRpc]
	private void RpcKillPlayer(string playerName) {
		ProcessKillPlayerMessage (playerName);
		ClientSceneManager.Instance.lastGameResults.deathList.Add (playerName);
	}

	[Server]
	private void ServerKillPlayer(string playerName) {
		ProcessKillPlayerMessage (playerName);
		ServerSceneManager.Instance.lastGameResults.deathList.Add (playerName);
	}


	private void ProcessKillPlayerMessage (string playerName) {
		deathList.Add (playerName);
	}

	[Server]
	private void CheckForGameOver () {
		if (deathList.Count >= (cars.Count - 1)) {
			ServerSceneManager.Instance.onGameEnd ();
		}
	}


	public void AddCar(GameObject gamePlayer)
	{
		cars.Add(gamePlayer.GetComponent<CarController>());
	}
		
}