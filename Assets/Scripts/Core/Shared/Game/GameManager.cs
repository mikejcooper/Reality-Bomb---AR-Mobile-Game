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

	private CarList _cars = new CarList();

	private bool _preparingGame = true;

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
				ServerSceneManager.Instance.OnPlayerDisconnectEvent += OnPlayerDisconnected;
			}
		}
			
		// use downloaded marker pattern
		MeshTransferManager.ApplyMarkerData (MarkerComponent);

		WorldMesh.transform.parent = MarkerScene.transform;



		if (OnWorldMeshAvailableEvent != null)
			OnWorldMeshAvailableEvent (WorldMesh);


		foreach (var existingCarController in GameObject.FindObjectsOfType<CarController>()) {
			existingCarController.init ();
		}

		if (!isServer) {
			ClientSceneManager.Instance.OnGameLoaded ();
		}
	}
		
	private void Update ()
	{
		if (_preparingGame) {
			return;
		}
		_cars.TickTime (Time.deltaTime);
		if (isServer) {
			foreach(CarController car in _cars.GetCarsOutOfTime()){
				KillPlayer (car);
			}
			if (_cars.GetNumberOfBombsPresent() == 0) {
				_cars.ClearAllDisconnectedPlayers ();
				_cars.PassBombRandomPlayer ();
			}
		}
	}
		
	void OnDestroy () {
		// isServer is unset by NetworkIdentity before our OnDestroy
		// so we have to check whether server is running instead.
		if (NetworkServer.active) {
			ServerSceneManager.Instance.OnPlayerDisconnectEvent -= OnPlayerDisconnected;
			ServerSceneManager.Instance.OnPlayerGameLoadedEvent -= CheckAreAllPlayersGameLoaded;
		}
	}

	[Server]
	private void KillPlayer (CarController car) {
		_cars.KillPlayer (car);
		CheckForGameOver ();
		_cars.PassBombRandomPlayer();
	}

	[Server]
	private void CheckForGameOver () {
		if (_cars.GetNumberAliveCars() == 1) {
			_cars.FinaliseGamePlayerData();
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
		_preparingGame = false;
		_cars.PassBombRandomPlayer ();
	}

	private void CheckAreAllPlayersGameLoaded () {
		if (AreAllPlayersGameLoaded()) {
			AllPlayersReady();
		}
	}

	private bool AreAllPlayersGameLoaded () {
		foreach (var car in GameObject.FindObjectsOfType<CarController>()) {
			if (car != null && !car.HasLoadedGame) {
				return false;
			}
		}
		return true;
	}



	public void AddCar(GameObject gamePlayer)
	{
		_cars.AddCar(gamePlayer.GetComponent<CarController>());
	}
		
	[Server]
	public void CollisionEvent(CarController car, Collision col){
		CarController collisionCar = col.gameObject.GetComponent<CarController>();
		if (col.gameObject.tag == "TankTag") {
			if ( collisionCar.IsTransferTimeExpired() && collisionCar.HasBomb )
			{
				collisionCar.setBombAllDevices(false);
				car.setBombAllDevices (true);
				car.UpdateTransferTime (1.0f);
			} 
		}
	} 
		
	[Server]
	public void OnPlayerDisconnected(){
		_cars.ClearAllDisconnectedPlayers ();
		CheckForGameOver ();
		if (_cars.GetNumberOfBombsPresent() < 1) _cars.PassBombRandomPlayer ();
	}


}