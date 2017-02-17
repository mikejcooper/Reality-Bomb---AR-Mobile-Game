using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameObject worldMesh);

	public event OnWorldMeshAvailable OnWorldMeshAvailableEvent;

	public GameObject MarkerScene;
	public int BombPlayerConnectionId;

	private List<CarController> _cars = new List<CarController>();

	private List<string> _deathList = new List<string>();

	public GameObject WorldMesh { get; private set; }

	void Start ()
	{
		if (!isServer) {
			ClientSceneManager.Instance.LastGameResults = new GameResults ();

			WorldMesh = ClientSceneManager.Instance.WorldMesh;
		} else if (isServer) {
			ServerSceneManager.Instance.LastGameResults = new GameResults ();

			BombPlayerConnectionId = GameUtils.ChooseRandomPlayerConnectionId ();
			DebugConsole.Log ("=> bombPlayerConnectionId: " + BombPlayerConnectionId);

			WorldMesh = ServerSceneManager.Instance.WorldMesh;
		}

		WorldMesh.transform.parent = MarkerScene.transform;


		if (OnWorldMeshAvailableEvent != null)
			OnWorldMeshAvailableEvent (WorldMesh);
		

		foreach (var existingCarController in GameObject.FindObjectsOfType<CarController>()) {
			existingCarController.init ();
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
		Debug.LogError (string.Format ("AddCar id: {0}", gamePlayer.GetInstanceID()));
		_cars.Add(gamePlayer.GetComponent<CarController>());
	}
		
}