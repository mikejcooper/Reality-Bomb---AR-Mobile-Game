﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ServerLifecycle;
using TMPro;
using Powerups;


/*
* REMEMBER CANT USE CLIENTRPC ATTRIBUTE IN GAMEMANAGER
* ClientRpcs can only be run on objects that have been spawned via NetworkServer.Spawn(). Do not run RPC from a static gameobject.
*/

public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameObject worldMesh);
	public event OnWorldMeshAvailable OnWorldMeshAvailableEvent = delegate {};

	//public delegate void StartGameCountDown();
	//public event StartGameCountDown StartGameCountDownEvent = delegate {};

	//public delegate void OnGameStarted();
	//public event OnGameStarted OnGameStartedEvent = delegate {};

	public PreparingGame PreparingCanvas;
	public GameObject MarkerScene;
	public ARMarker MarkerComponent;
	public GameObject BombObject;


	private CarList _cars = new CarList();

	private bool _preparingGame = true;

	public GameObject WorldMesh { get; private set; }


	void Start ()
	{
		if (!isServer) {
			WorldMesh = ClientSceneManager.Instance.WorldMesh;


		} else if (isServer) {

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
        if (GlobalNetworking.NetworkConstants.SCANNED_MARKERS)
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
        /*
		if (StartGameCountDownEvent != null) {
			StartGameCountDownEvent();
		}
        */
//		RpcAllPlayersReady ();

		PreparingCanvas.StartGameCountDown (); //Starts countdown on server
		_cars.StartGameCountDown (); //Starts countdown on clients using RPC
		Debug.Log ("SERVER GAME COUNT DOWN");
	}

    /*
	[ClientRpc]
	private void RpcAllPlayersReady(){
		if (StartGameCountDownEvent != null) {
			StartGameCountDownEvent();
		}
	}
    */
		
	[Server]
	public void CountDownFinishedStartPlaying(){
		_preparingGame = false;
		Debug.Log ("COUNTDOWNFINISHED");
        /*
		if (OnGameStartedEvent != null) {
			OnGameStartedEvent();
		}
        */
		_cars.enableAllControls();
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
		Debug.Log ("Player Disconnected");
		Debug.Log ("Players Left: " + _cars.GetCarsOutOfTime() + _cars.GetNumberAliveCars());
		_cars.ClearAllDisconnectedPlayers ();
		Debug.Log ("Players Left: " + _cars.GetCarsOutOfTime() + _cars.GetNumberAliveCars());
		CheckForGameOver ();
		_cars.PassBombRandomPlayer ();
	}



}