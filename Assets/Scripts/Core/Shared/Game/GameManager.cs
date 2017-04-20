using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ServerLifecycle;
using TMPro;
using Powerups;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour {

	public delegate void OnWorldMeshAvailable(GameMapObjects worldMesh);
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

	public GameObject Canvas;

	private const int EXPLANATION_DIALOG_DELAY = 2;

	private CarList _cars = new CarList();

	private bool _allPlayersReady = false;
	private bool _preparingGame = true;

	public int _startingBombPlayerConnectionId;
	private GameObject _clientExplanationDialog;

	public GameMapObjects WorldMesh { get; private set; }


	void Start ()
	{
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

		}
			
		// use downloaded marker pattern
		MeshTransferManager.ApplyMarkerData (MarkerComponent);

		WorldMesh.ground.transform.parent = MarkerScene.transform;
		WorldMesh.boundary.transform.parent = MarkerScene.transform;



		if (OnWorldMeshAvailableEvent != null) {
			OnWorldMeshAvailableEvent (WorldMesh);
		}


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
	}

	[Server]
	public void StartCountdown () {
		//Need to make sure _cars is populated at this point
		foreach(CarController car in FindObjectsOfType<CarController>())
		{
			AddCar(car.gameObject);
		}

		RpcOnBeginCountdown ();
		PreparingCanvas.StartGameCountDown (true);

		Debug.Log ("SERVER GAME COUNT DOWN");
	}

	[ClientRpc]
	public void RpcOnBeginCountdown () {
		Debug.Log ("RPC GAME COUNT DOWN");
		if (_clientExplanationDialog != null) {
			Destroy (_clientExplanationDialog);
		}

		StartCoroutine(FadeOutMesh(5));
		PreparingCanvas.StartGameCountDown (false);
	}
		
	IEnumerator FadeOutMesh(int duration)
	{
		int steps = 100;
		float timeInterval = duration / (float) steps;

		var material = WorldMesh.ground.GetComponent<MeshRenderer> ().material;

		float sourceAlpha = material.GetFloat ("_Alpha");
		float targetAlpha = 0f;

		float sourceSpeed = material.GetFloat ("_Speed");
		float targetSpeed = 0f;

		float alphaDec = (sourceAlpha - targetAlpha) / (float) steps;
		float speedDec = (sourceSpeed - targetSpeed) / (float) steps;



		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

		for (int i=0; i<steps; i++)
		{ 
			material.SetFloat ("_Alpha", sourceAlpha - i*alphaDec);
			material.SetFloat ("_Speed", sourceSpeed - i*speedDec);
			yield return new WaitForSeconds(timeInterval);
		}

		WorldMesh.ground.GetComponent<MeshRenderer> ().enabled = false;

	}

		
	[Server]
	public void CountDownFinishedStartPlaying(){
		_preparingGame = false;
		Debug.Log ("COUNTDOWNFINISHED");
		if (OnGameStartedEvent != null) {
			OnGameStartedEvent();
		}
		_cars.enableAllControls();
        if(_cars.GetNumberOfBombsPresent() < 1) _cars.PassBombRandomPlayer ();

		//Play the game music on the server only
		GameObject.FindObjectOfType<GameMusic>().StartMusic ();
	}

	public void AddCar(GameObject gamePlayer)
	{
		_cars.AddCar(gamePlayer.GetComponent<CarController>());
	}

    [Server]
    public void CollisionEvent(CarController car, GameObject other)
    {

        //this is two cars colliding
        CarController collisionCar = other.GetComponent<CarController>();
		if (other.CompareTag ("Car")) {
			if (collisionCar.IsTransferTimeExpired () && collisionCar.HasBomb) {
				collisionCar.setBombAllDevices (false);
				car.setBombAllDevices (true);
				car.UpdateTransferTime (1.0f);
			}
		} else if (other.CompareTag("PowerUp")) {
			//Handle powerups on the CarController clients
			_cars.TriggerPowerup (PowerUpManager.GetPowerupType (other, car.HasBomb), car.ServerId);
			//Destroy the gameobject we collided with (because it's a powerup)
			NetworkServer.Destroy (other);
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