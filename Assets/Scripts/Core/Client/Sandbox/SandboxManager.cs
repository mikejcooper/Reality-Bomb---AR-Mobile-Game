﻿using System.Collections;
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
	public GameObject TxtObjectPrefab;
	public Canvas CanvasObj;
	public List<GameObject> TutorialDialogPrefabs;

	private int _currentTutorialStage = 0;
	private GameObject _currentDialog;

//	private string _welcome_Txt = "\tWelcome to the Tutorial!\nUse the joystick to drive around but don't fall off the map!\nSee if you can pickup some Power Ups along the way!";
	private string _splatter_Txt = "You've Activated the Splatter Power Up!\nThis splatters ink on your opponents' screens\nas shown below making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost Power Up!\nEnjoy double the speed but be careful not to lose control!";
	private string _respawn_Txt = "Oops!! You fell off the map! Don't Worry, You will\nbe respawned but you wont be able to move for 5 secs.\nBe careful or become an easy target!";


	private Rect TxtRect;

	void Start(){
		if (TutorialDialogPrefabs.Count > 0) {
			SetCurrentDialog (0);
		}

		Debug.LogError ("TutorialDialogPrefabs.count: " + TutorialDialogPrefabs.Count);

//		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _welcome_Txt;
		SandBoxPowerUpManager.SpeedBoostActivatedEvent += SetSpeedTxt;
		SandBoxPowerUpManager.InkSplatterActivatedEvent += SetSplatTxt;
	}

	void OnDestroy(){
		SandBoxPowerUpManager.SpeedBoostActivatedEvent -= SetSpeedTxt;
		SandBoxPowerUpManager.InkSplatterActivatedEvent -= SetSplatTxt;
	}

	private void SetCurrentDialog (int index) {
		_currentTutorialStage = index;
		_currentDialog = GameObject.Instantiate (TutorialDialogPrefabs [index]);
		_currentDialog.transform.SetParent (CanvasObj.transform, false);
		_currentDialog.gameObject.GetComponentInChildren<Button> ().onClick.AddListener (() => {
			if (_currentTutorialStage + 1 < TutorialDialogPrefabs.Count) {
				var oldDialog = _currentDialog;
				SetCurrentDialog(_currentTutorialStage + 1);
				Destroy(oldDialog);
			} else {
				Destroy(_currentDialog);
				OnModalTutorialEnd();
			}
		});
	}

	private void OnModalTutorialEnd () {
		
	}

	void Update(){
		EnsureCarIsOnMap ();
	}
		
	private void ClearGuiTxt(){
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = "";
	}

	public void EnsureCarIsOnMap(){
		if(CarObject.transform.position.y <= - 10.0f){
			Reposition (PlaneObject);
		}
	}
		
	public void Reposition(GameObject worldMesh)
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
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _splatter_Txt;
		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetSpeedTxt(){
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _speed_Txt; 
		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetRespawnTxt(){
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _respawn_Txt;
		Invoke ("ClearGuiTxt", 10.0f);
	}


}


