﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ServerLifecycle;



public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameObject worldMesh);


	public event OnWorldMeshAvailable OnWorldMeshAvailableEvent;

	public PreparingGame PreparingCanvas;
	public GameObject MarkerScene;
	public ARMarker MarkerComponent;
	public GameObject BombObject;

	private List<CarController> _cars = new List<CarController>();
	private List<CarController> _remainingCars = new List<CarController>();

	private List<string> _deathList = new List<string>();
	public int _startingBombPlayerConnectionId;

	public GameObject WorldMesh { get; private set; }




	void Start ()
	{
		if (!isServer) {
			ClientSceneManager.Instance.LastGameResults = new GameResults ();
			WorldMesh = ClientSceneManager.Instance.WorldMesh;


		} else if (isServer) {
			ServerSceneManager.Instance.LastGameResults = new GameResults ();

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


		if (OnWorldMeshAvailableEvent != null)
			OnWorldMeshAvailableEvent (WorldMesh);


		foreach (var existingCarController in GameObject.FindObjectsOfType<CarController>()) {
			existingCarController.init ();
		}

		if (!isServer) {
			ClientSceneManager.Instance.OnGameLoaded ();
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

	public bool IsStartingBomb (int connectionId) {
		return connectionId == _startingBombPlayerConnectionId;
	}


	void OnDestroy () {
		if (isServer) {
			ServerSceneManager.Instance.OnPlayerGameLoadedEvent -= CheckAreAllPlayersGameLoaded;
		}
	}

	[Server]
	private string GetPlayerName (CarController car) {
		return string.Format ("player {0}", car.connectionToClient.connectionId);
	}

	[Server]
	public void AllDevicesKillPlayer (CarController car) {
		string playerName = GetPlayerName (car);
		_deathList.Add (playerName);
		RpcKillPlayer (playerName);
		ServerKillPlayer (playerName);
		CheckForGameOver ();
		PassBombRandomPlayer ();
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
		RemoveByName (_remainingCars, playerName);
	}

	[Server]
	private void CheckForGameOver () {
		if (_deathList.Count >= (_cars.Count - 1)) {
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
	private void CountDownFinishedStartPlaying(){
		EnableAllControls (true);
		CarController server = GameObject.FindObjectOfType<CarController>();
		server.ServerGameStarting ();

	}

	[Server]
	public void EnableAllControls(bool boolean){
		foreach (CarController car in FindObjectsOfType<CarController>()) {
			car.RpcEnableALLControls (true);
			DebugConsole.Log ("Enabled");
		}
	}



	public void AddCar(GameObject gamePlayer)
	{
		_cars.Add(gamePlayer.GetComponent<CarController>());
		_remainingCars.Add(gamePlayer.GetComponent<CarController>());
	}

	private void PassBombRandomPlayer(){
		Debug.Log ("Bomb is passed to new random player");
		CarController randCar = _remainingCars[Random.Range(0,_remainingCars.Count)];
		randCar.AllDevicesSetBomb (true);
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
	public void OnPlayerDisconnected(){
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

	public void ServerMeshPlayerReady(){
		foreach (PreparingGame prep in gameObject.GetComponents<PreparingGame>()) {
			prep.StartGameCountDown ();
		}
		foreach (CarController car in _cars) {
			car.EnableControls (true);
		}
	}

		
}