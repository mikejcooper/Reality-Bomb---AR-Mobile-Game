using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Powerups;
using System.Collections.Generic;


public class SandboxManager : MonoBehaviour
{

	public GameObject CarObject;
	public GameObject PlaneObject;
	public Canvas CanvasObj;
	public ToastManager ToastManagerObject;
	public SandBoxPowerUpManager PowerUpManager;
	public List<GameObject> TutorialDialogPrefabs;
	public GameObject GameCountdownDialogPrefab;
	public Garage Garage;

	private int _currentTutorialStage = 0;
	private GameObject _currentTutorialDialog;
	private GameObject _countdownDialog;

	private string _splatter_Txt = "You've Activated the Ink Splatter power up! This splatters ink on your opponents' screens as shown below making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost power up! Enjoy double the speed but be careful not to lose control!";
	private string _respawn_Txt = "Oops!! You fell off the map! Don't Worry, You'll be respawned but you won't be able to move for 5s. Be careful or become an easy target!";
	private string _shield_Txt = "You've activated the Shield power up! While you have it, no one can touch you!";
	private string _grow_Txt = "You've been Super Sized! Your opponents don't stand a chance!";
	private string _shrink_Txt = "You've been Shrunk! You're enemies will have a hard time trying to see you.";

	void Start(){
		if (TutorialDialogPrefabs.Count > 0) {
			SetCurrentDialog (0);
			PowerUpManager.enabled = false;
		}

		if (ClientSceneManager.Instance != null) {
			StartCoroutine (ApplyLobbyPlayerCarProperties ());
		} else {
			Garage.ApplyVehicleToShell (Garage.AvailableVehicles [1], CarObject, 320);
		}
			
		PowerUpManager.SpeedBoostActivatedEvent += SetSpeedTxt;
		PowerUpManager.InkSplatterActivatedEvent += SetSplatTxt;
		PowerUpManager.ShieldActivatedEvent += SetShieldTxt;
		PowerUpManager.GrowActivatedEvent += SetGrowTxt;
		PowerUpManager.ShrinkActivatedEvent += SetShrinkTxt;

		if(ClientSceneManager.Instance != null){
			ClientSceneManager.Instance.OnCountDownTimeUpdateEvent += OnCountDownTimeUpdate;
			ClientSceneManager.Instance.OnCountDownCanceledEvent += OnCountDownCanceled;
		}

	}

	// I can't think of a better way of piping the evtn through right now...
	IEnumerator ApplyLobbyPlayerCarProperties()
	{
		NetworkCompat.NetworkLobbyPlayer player = ClientSceneManager.Instance.GetOwnLobbyPlayer ();
		while(player == null) 
		{ 
			player = ClientSceneManager.Instance.GetOwnLobbyPlayer ();
			yield return new WaitForSeconds(0.5f);
		}
		player.OnCarDetailsUpdateEvent += () => {
			NetworkCompat.NetworkLobbyPlayer newPlayer = ClientSceneManager.Instance.GetOwnLobbyPlayer ();
			Debug.Log(string.Format("adding car shell for vehicle id: {0}", newPlayer.vehicleId));
			Garage.ApplyVehicleToShell (newPlayer.vehicleId, CarObject, newPlayer.colour);
		};

		Debug.Log(string.Format("adding initial car shell for vehicle id: {0}", player.vehicleId));
		Garage.ApplyVehicleToShell (player.vehicleId, CarObject, player.colour);	

		while(true) 
		{ 
			Debug.Log(string.Format("checking name: {0} vehicle id: {1}", ClientSceneManager.Instance.GetOwnLobbyPlayer ().nickname, ClientSceneManager.Instance.GetOwnLobbyPlayer ().vehicleId));
			yield return new WaitForSeconds(5);
		}
	}

	void OnDestroy(){
		PowerUpManager.SpeedBoostActivatedEvent -= SetSpeedTxt;
		PowerUpManager.InkSplatterActivatedEvent -= SetSplatTxt;
		PowerUpManager.ShieldActivatedEvent -= SetShieldTxt;
		PowerUpManager.GrowActivatedEvent -= SetGrowTxt;
		PowerUpManager.ShrinkActivatedEvent -= SetShrinkTxt;

		if (ClientSceneManager.Instance != null) {
			ClientSceneManager.Instance.OnCountDownTimeUpdateEvent -= OnCountDownTimeUpdate;
			ClientSceneManager.Instance.OnCountDownCanceledEvent -= OnCountDownCanceled;
		}
	}

	private void OnCountDownTimeUpdate (int remainingTime) {
		if (_countdownDialog == null) {
			_countdownDialog = GameObject.Instantiate (GameCountdownDialogPrefab);
			_countdownDialog.transform.SetParent (CanvasObj.transform, false);
			_countdownDialog.GetComponentInChildren<AlarmTextDriver> ().OnCountDownTimeUpdate (remainingTime);
		}
	}

	private void OnCountDownCanceled (string reason) {
		if (_countdownDialog != null) {
			Destroy (_countdownDialog);
			_countdownDialog = null;
		}
	}

	private void SetCurrentDialog (int index) {
		_currentTutorialStage = index;
		_currentTutorialDialog = GameObject.Instantiate (TutorialDialogPrefabs [index]);
		_currentTutorialDialog.transform.SetParent (CanvasObj.transform, false);
		_currentTutorialDialog.gameObject.GetComponentInChildren<Button> ().onClick.AddListener (() => {
			if (_currentTutorialStage + 1 < TutorialDialogPrefabs.Count) {
				var oldDialog = _currentTutorialDialog;
				SetCurrentDialog(_currentTutorialStage + 1);
				Destroy(oldDialog);
			} else {
				Destroy(_currentTutorialDialog);
				OnModalTutorialEnd();
			}
		});
	}

	private void OnModalTutorialEnd () {
		PowerUpManager.enabled = true;
	}

	void Update(){
		EnsureCarIsOnMap ();
	}

	public void EnsureCarIsOnMap(){
		if(CarObject.transform.position.y <= - 10.0f){
			Reposition();
		}
	}
		
	public void Reposition()
	{
		Debug.Log ("Repositioning car");
		SetRespawnTxt ();
		var car_rigid = CarObject.GetComponent<Rigidbody> ();
		//Set velocities to zero
		car_rigid.velocity = Vector3.zero;
		car_rigid.angularVelocity = Vector3.zero;

		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);

		if (position != Vector3.zero) {
			Debug.Log ("unfreezing");

			gameObject.SetActive (true);
			car_rigid.isKinematic = false;
			car_rigid.position = position;
		}

	}

	public void SetSplatTxt(){
		ToastManagerObject.ShowMessage(_splatter_Txt);
	}

	public void SetSpeedTxt(){
		ToastManagerObject.ShowMessage(_speed_Txt);
	}

	public void SetRespawnTxt(){
		ToastManagerObject.ShowMessage(_respawn_Txt);
	}

	public void SetShieldTxt(){
		ToastManagerObject.ShowMessage(_shield_Txt);
	}

	public void SetGrowTxt(){
		ToastManagerObject.ShowMessage(_grow_Txt);
	}

	public void SetShrinkTxt(){
		ToastManagerObject.ShowMessage(_shrink_Txt);
	}

}


