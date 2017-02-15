using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public int BombPlayerConnectionId;

	private List<CarController> _cars = new List<CarController>();

	private List<string> _deathList = new List<string>();


	private static GameManager _instance;

	public static GameManager Instance { get { return _instance; } }


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
			ClientSceneManager.Instance.LastGameResults = new GameResults ();
		} else if (isServer) {
			ServerSceneManager.Instance.LastGameResults = new GameResults ();

			List<int> activeConnectionIds = new List<int>();
			for (int i=0; i<UnityEngine.Networking.NetworkServer.connections.Count; i++) {
				if (UnityEngine.Networking.NetworkServer.connections [i] != null &&
				    UnityEngine.Networking.NetworkServer.connections [i].isConnected) {
					activeConnectionIds.Add (UnityEngine.Networking.NetworkServer.connections [i].connectionId);
				}
			}
			int bombPlayerIndex = Random.Range(0, activeConnectionIds.Count);
			DebugConsole.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, activeConnectionIds.Count));
			BombPlayerConnectionId = activeConnectionIds [bombPlayerIndex];
			DebugConsole.Log ("=> bombPlayerConnectionId: " + BombPlayerConnectionId);
		}
	}


	[Server]
	private string GetPlayerName (CarController car) {
		return string.Format ("player {0}", car.connectionToClient.connectionId);
	}

	[Server]
	public void AllDevicesKillPlayer (CarController car) {
		string playerName = GetPlayerName (car);
		RpcKillPlayer (playerName);
		ServerKillPlayer (playerName);
		CheckForGameOver ();
	}

	[ClientRpc]
	private void RpcKillPlayer(string playerName) {
		ProcessKillPlayerMessage (playerName);
		ClientSceneManager.Instance.LastGameResults.DeathList.Add (playerName);
	}

	[Server]
	private void ServerKillPlayer(string playerName) {
		ProcessKillPlayerMessage (playerName);
		ServerSceneManager.Instance.LastGameResults.DeathList.Add (playerName);
	}


	private void ProcessKillPlayerMessage (string playerName) {
		_deathList.Add (playerName);
	}

	[Server]
	private void CheckForGameOver () {
		if (_deathList.Count >= (_cars.Count - 1)) {
			ServerSceneManager.Instance.OnServerRequestGameEnd ();
		}
	}


	public void AddCar(GameObject gamePlayer)
	{
		_cars.Add(gamePlayer.GetComponent<CarController>());
	}
		
}