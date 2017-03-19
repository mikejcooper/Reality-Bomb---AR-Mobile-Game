using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ServerLifecycle;
using TMPro;


public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameObject worldMesh);


	public event OnWorldMeshAvailable OnWorldMeshAvailableEvent = delegate {};

	public PreparingGame PreparingCanvas;
	public GameObject MarkerScene;
	public ARMarker MarkerComponent;
	public GameObject BombObject;

	private List<CarController> _cars = new List<CarController>();
	private List<CarController> _remainingCars = new List<CarController>();

	private int _deathCounter = 0;
	public int _startingBombPlayerConnectionId;

	public GameObject WorldMesh { get; private set; }



	void Start ()
	{

		if (isServer) {
			if (GameObject.Find ("JoystickBack") != null) {
				GameObject.Find ("JoystickBack").SetActive (false);
			}
			if (GameObject.Find ("HealthBar") != null) {
				GameObject.Find ("HealthBar").SetActive (false);
			}
			if (GameObject.Find ("SpectatingText") != null) {
				GameObject.Find ("SpectatingText").GetComponent<TextMeshProUGUI>().text = "Spectating...";
			}
		}

		if (!isServer) {
			WorldMesh = ClientSceneManager.Instance.WorldMesh;


		} else if (isServer) {

			_startingBombPlayerConnectionId = GameUtils.ChooseRandomPlayerConnectionId ();
			DebugConsole.Log ("=> bombPlayerConnectionId: " + _startingBombPlayerConnectionId);

			WorldMesh = ServerSceneManager.Instance.WorldMesh;

			PreparingCanvas.CountDownFinishedEvent += new PreparingGame.CountDownFinished (CountDownFinishedStartPlaying);
			if (AreAllPlayersGameLoaded ()) {
				AllPlayersReady ();
			} else {
				ServerSceneManager.Instance.OnPlayerGameLoadedEvent += CheckAreAllPlayersGameLoaded;
			}
		}
			
		// use downloaded marker pattern
		MeshTransferManager.ApplyMarkerData (MarkerComponent);

		WorldMesh.transform.parent = MarkerScene.transform;

//		ServerSceneManager.Instance.OnPlayerDisconnectEvent += OnPlayerDisconnected;

		if (OnWorldMeshAvailableEvent != null)
			OnWorldMeshAvailableEvent (WorldMesh);


		foreach (var existingCarController in GameObject.FindObjectsOfType<CarController>()) {
			existingCarController.init ();
		}

		if (!isServer) {
			ClientSceneManager.Instance.OnGameLoaded ();
		}
	}

	public bool IsStartingBomb (int connectionId) {
		return connectionId == _startingBombPlayerConnectionId;
	}


	void OnDestroy () {
		// isServer is unset by NetworkIdentity before our OnDestroy
		// so we have to check whether server is running instead.
		if (NetworkServer.active) {
//			ServerSceneManager.Instance.OnPlayerDisconnectEvent -= OnPlayerDisconnected;
			ServerSceneManager.Instance.OnPlayerGameLoadedEvent -= CheckAreAllPlayersGameLoaded;
		}
	}

	[Server]
	private string GetPlayerName (CarController car) {
		return string.Format ("player {0}", car.connectionToClient.connectionId);
	}

	[Server]
	public void KillPlayer (CarController car) {
		_deathCounter++;
		int carsLeft = _cars.Count - _deathCounter;
		ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, carsLeft, car.Lifetime);
		CheckForGameOver ();
//		NetworkServer.Destroy (car.gameObject);
//		KillDisconnectedPlayer ();
		PassBombRandomPlayer ();
	}

	private void ProcessKillPlayerMessage (string playerName) {
		RemoveByName (_remainingCars, playerName);
	}

	[Server]
	private void CheckForGameOver () {
		if (_deathCounter >= (_cars.Count - 1)) {
			// update game data for survivor
			foreach (var car in _cars) {
				if (car.Alive) {
					ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, 0, car.Lifetime);
					break;
				}
			}


			ServerSceneManager.Instance.OnServerRequestGameEnd ();
		}
	}
		
	[Server]
	private void AllPlayersReady(){
		Debug.Log ("Server: All player are ready, start game countdown");
		RpcPlayerReady ();
		PreparingCanvas.StartGameCountDown ();
	}

	[ClientRpc] // All players ready (synced), start countdown 
	public void RpcPlayerReady() {
		Debug.Log ("Client: All player are ready, start game countdown ");
		PreparingCanvas.StartGameCountDown ();
	}
		
	[Server]
	public void CountDownFinishedStartPlaying(){
		foreach (CarController car in FindObjectsOfType<CarController>()) {
			car.RpcPlayerGameStarting ();
			car.ServerGameStarting ();
		}
	}

	public void CheckAreAllPlayersGameLoaded () {
		if (AreAllPlayersGameLoaded()) {
			AllPlayersReady();
		}
	}

	public bool AreAllPlayersGameLoaded () {
		foreach (var car in GameObject.FindObjectsOfType<CarController>()) {
			if (car != null && !car.HasLoadedGame) {
				return false;
			}
		}
		return true;
	}



	public void AddCar(GameObject gamePlayer)
	{
		_cars.Add(gamePlayer.GetComponent<CarController>());
		_remainingCars.Add(gamePlayer.GetComponent<CarController>());
	}

	private void PassBombRandomPlayer(){
		Debug.Log ("Bomb is passed to new random player");
		foreach (CarController car in _cars) {
			if (car.Alive && !car.HasBomb) {
				car.AllDevicesSetBomb (true);
				return;
				}
			}
	}

	private void RemoveByName( List<CarController> list, string name){
		CarController tempCar = null;
		foreach (CarController car in list) {
			if (GetPlayerName (car).Equals (name)) {
				tempCar = car;
			}
		}
		if (tempCar != null) {
			list.Remove (tempCar);
		}
	}

	[Server]
	public void CollisionEvent(CarController car, Collision col){
		CarController collisionCar = col.gameObject.GetComponent<CarController>();

		if (col.gameObject.tag == "TankTag") {
			if ( collisionCar.IsTransferTimeExpired() && collisionCar.HasBomb )
			{
				collisionCar.AllDevicesSetBomb(false);
				car.AllDevicesSetBomb (true);
				car.UpdateTransferTime (1.0f);
			} 
		}
	} 

	[Server]
	private void KillDisconnectedPlayer(){
		DebugConsole.Log ("Gamemanager Kill DisconnectedPlayer");
		for(var i = _cars.Count - 1; i > -1; i--)
		{
			if (_cars[i].connectionToClient == null)_cars.RemoveAt(i);
		}
		CheckForGameOver ();
		if (!IsBombInGame ()) PassBombRandomPlayer ();
	}

	[Server]
	public void OnPlayerDisconnected(){
//		KillDisconnectedPlayer ();
		if(!IsBombInGame()){
			PassBombRandomPlayer ();
		}
	}

	private bool IsBombInGame(){
		foreach (CarController car in _cars) {
			if (car.HasBomb) {
				return true;
			}
		}
		return false;
	}

}