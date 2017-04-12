using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ServerLifecycle;
using TMPro;
using Powerups;
using UnityEngine.UI;

/*
* REMEMBER CANT USE CLIENTRPC ATTRIBUTE IN GAMEMANAGER
* ClientRpcs can only be run on objects that have been spawned via NetworkServer.Spawn(). Do not run RPC from a static gameobject.
*/

public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameObject worldMesh);
	public event OnWorldMeshAvailable OnWorldMeshAvailableEvent = delegate {};

	public delegate void StartGameCountDown();
	public event StartGameCountDown StartGameCountDownEvent = delegate {};

	public delegate void OnGameStarted();
	public event OnGameStarted OnGameStartedEvent = delegate {};

	public PreparingGame PreparingCanvas;
	public GameObject MarkerScene;
	public ARMarker MarkerComponent;
	public GameObject BombObject;
	public GamePowerUpManager PowerUpManager;
	public GameObject GameExplanationDialogPrefab;
	public GameObject GameStartingDialogObj;

	// todo: get rid of this once we transition to keyboard shortcuts
	public GameObject ButtonPrefab;
	public GameObject Canvas;

	private const int EXPLANATION_DIALOG_DELAY = 2;

	private Button _serverStartButton;
	private CarList _cars = new CarList();

	private bool _allPlayersReady = false;
	private bool _preparingGame = true;

	public int _startingBombPlayerConnectionId;
	private GameObject _clientExplanationDialog;

	public GameObject WorldMesh { get; private set; }



	void Start ()
	{
		PowerUpManager.enabled = false;
		if (!isServer) {
			WorldMesh = ClientSceneManager.Instance.WorldMesh;

			Invoke ("ShowExplanationDialog", EXPLANATION_DIALOG_DELAY);
			 
		} else if (isServer) {

			_startingBombPlayerConnectionId = GameUtils.ChooseRandomPlayerConnectionId ();
			Debug.Log ("=> bombPlayerConnectionId: " + _startingBombPlayerConnectionId);

			WorldMesh = ServerSceneManager.Instance.WorldMesh;

			PreparingCanvas.CountDownFinishedEvent += new PreparingGame.CountDownFinished (CountDownFinishedStartPlaying);

            //Triggered when last player loads game scene
			ServerSceneManager.Instance.OnAllPlayersLoadedEvent += AllPlayersReady;
			ServerSceneManager.Instance.OnPlayerDisconnectEvent += OnPlayerDisconnected;

			SetServerUI ();
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

	private void ShowExplanationDialog () {
		Destroy (GameStartingDialogObj);
		GameStartingDialogObj = null;

		_clientExplanationDialog = GameObject.Instantiate (GameExplanationDialogPrefab);
		_clientExplanationDialog.transform.SetParent (Canvas.transform, false);
		_clientExplanationDialog.gameObject.GetComponentInChildren<Button> ().onClick.AddListener (() => {
			Destroy(_clientExplanationDialog);
		});
	}

	[Server]
	private void SetServerUI () {

		var joystick = GameObject.FindObjectOfType<Joystick> ();
		if (joystick != null) {
			joystick.gameObject.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find joystick. Check this!");
		}

		var healthbar = GameObject.Find ("HealthBar");
		if (healthbar != null) {
			healthbar.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find health bar. Check this!");
		}

		var markerAlert = GameObject.Find ("MarkerAlert");
		if (markerAlert != null) {
			markerAlert.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find marker alert. Check this!");
		}

		Destroy (GameStartingDialogObj);
		GameStartingDialogObj = null;

		// this is pretty hacky dev stuff
		GameObject button = GameObject.Instantiate (ButtonPrefab);

		button.transform.parent = Canvas.transform;

		button.GetComponentInChildren<UnityEngine.UI.Text> ().text = "Start";
		_serverStartButton = button.GetComponent<UnityEngine.UI.Button> ();
		_serverStartButton.enabled = false;
		_serverStartButton.onClick.AddListener (() => {
			Debug.Log("click");
			Destroy(button);
			_serverStartButton = null;
			StartCountdown();
		});
		button.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;


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
			_cars.ClearAllDisconnectedPlayers ();
			if (_cars.GetNumberOfBombsPresent() == 0) {
				_cars.PassBombRandomPlayer ();
			}
		}
	}
		
	void OnDestroy () {
		// isServer is unset by NetworkIdentity before our OnDestroy
		// so we have to check whether server is running instead.
		if (NetworkServer.active) {
			ServerSceneManager.Instance.OnPlayerDisconnectEvent -= OnPlayerDisconnected;
            ServerSceneManager.Instance.OnAllPlayersLoadedEvent -= AllPlayersReady;
        }
	}

	[Server]
	private void KillPlayer (CarController car) {
		_cars.KillPlayer (car);
		CheckForGameOver ();
		if(_cars.GetNumberOfBombsPresent() == 0) _cars.PassBombRandomPlayer();
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
		
		_allPlayersReady = true;
		_serverStartButton.enabled = true;
	}

	[Server]
	private void StartCountdown () {
		//Need to make sure _cars is populated at this point
		foreach(CarController car in FindObjectsOfType<CarController>())
		{
			AddCar(car.gameObject);
		}
		_cars.StartGameCountDown();
		RpcEnsureExplanationDialogDismissed ();
		PreparingCanvas.StartGameCountDown (true);

		Debug.Log ("SERVER GAME COUNT DOWN");
	}

	[ClientRpc]
	public void RpcEnsureExplanationDialogDismissed () {
		if (_clientExplanationDialog != null) {
			Destroy (_clientExplanationDialog);
		}
	}
		
	[Server]
	public void CountDownFinishedStartPlaying(){
		_preparingGame = false;
		Debug.Log ("COUNTDOWNFINISHED");
		if (OnGameStartedEvent != null) {
			OnGameStartedEvent();
		}
		_cars.enableAllControls();
		PowerUpManager.enabled = true;
        if(_cars.GetNumberOfBombsPresent() < 1) _cars.PassBombRandomPlayer ();

		//Play the game music on the server only
		GameObject.FindObjectOfType<GameMusic>().StartMusic ();
	}

	public void AddCar(GameObject gamePlayer)
	{
		_cars.AddCar(gamePlayer.GetComponent<CarController>());
	}
		
	[Server]
	public void CollisionEvent(CarController car, Collision col){

		//this is two cars colliding
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
		if (_cars.GetNumberOfBombsPresent() < 1) _cars.PassBombRandomPlayer ();
	}



}