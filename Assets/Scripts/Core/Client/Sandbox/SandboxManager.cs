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
	public SandBoxPowerUpManager PowerupManager;
	public List<GameObject> TutorialDialogPrefabs;
	public GameObject GameCountdownDialogPrefab;

	private int _currentTutorialStage = 0;
	private GameObject _currentTutorialDialog;
	private GameObject _countdownDialog;

	private string _splatter_Txt = "You've Activated the Splatter Power Up! This splatters ink on your opponents' screens as shown below making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost Power Up! Enjoy double the speed but be careful not to lose control!";
	private string _respawn_Txt = "Oops!! You fell off the map! Don't Worry, You'll be respawned but you won't be able to move for 5s. Be careful or become an easy target!";

	void Start(){
		if (TutorialDialogPrefabs.Count > 0) {
			SetCurrentDialog (0);
			PowerupManager.enabled = false;
		}
			
		PowerupManager.SpeedBoostActivatedEvent += SetSpeedTxt;
		PowerupManager.InkSplatterActivatedEvent += SetSplatTxt;

		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent += OnCountDownTimeUpdate;
		ClientSceneManager.Instance.OnCountDownCanceledEvent += OnCountDownCanceled;
	}

	void OnDestroy(){
		PowerupManager.SpeedBoostActivatedEvent -= SetSpeedTxt;
		PowerupManager.InkSplatterActivatedEvent -= SetSplatTxt;

		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent -= OnCountDownTimeUpdate;
		ClientSceneManager.Instance.OnCountDownCanceledEvent -= OnCountDownCanceled;
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
		PowerupManager.enabled = true;
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
//		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _splatter_Txt;
		ToastManagerObject.ShowMessage(_splatter_Txt);
//		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetSpeedTxt(){
		ToastManagerObject.ShowMessage(_speed_Txt);
//		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _speed_Txt; 
//		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetRespawnTxt(){
		ToastManagerObject.ShowMessage(_respawn_Txt);
//		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _respawn_Txt;
//		Invoke ("ClearGuiTxt", 10.0f);
	}


}


