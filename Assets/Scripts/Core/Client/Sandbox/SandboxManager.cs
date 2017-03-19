using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class SandboxManager : MonoBehaviour
{

	public GameObject CarObject;
	public GameObject PlaneObject;
	public GameObject TxtObjectPrefab;
	public CanvasMessages CanvasMessage;


	private bool _controlsDisabled;
	private string _welcome_Txt = "\tWelcome to the Tutorial!\nUse the joystick to drive around but don't fall off the map!\nSee if you can pickup some Power Ups along the way!";
	private string _splatter_Txt = "You've Activated the Splatter Power Up!\nThis splatters ink on your opponents' screens\nas shown below making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost Power Up!\nEnjoy double the speed but becareful not to lose control!";
	private string _respawn_Txt = "Oops!! You fell off the map! Don't Worry, You will\nbe respawned but you wont be able to move for 5 secs.\nBecareful or become an easy target!";


	private Rect TxtRect;

	void Start(){
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _welcome_Txt;
		_controlsDisabled = false;
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
			DisableControls (5);
		}
	}

	public void DisableControls(int seconds){
		ToggleControls();
		Invoke("ToggleControls", seconds);
	}

	public void ToggleControls(){
		_controlsDisabled = !_controlsDisabled;
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
			DebugConsole.Log ("unfreezing");

			gameObject.SetActive (true);
			car_rigid.isKinematic = false;
			car_rigid.position = position;
		}

	}

	public void SetSplatTxt(){
		CanvasMessage.DisplayPowerUpMessage ("Ink Splatter");
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _splatter_Txt;
		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetSpeedTxt(){
		CanvasMessage.DisplayPowerUpMessage ("Speed Boost");
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _speed_Txt; 
		Invoke ("ClearGuiTxt", 10.0f);
	}

	public void SetRespawnTxt(){
		TxtObjectPrefab.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = _respawn_Txt;
		Invoke ("ClearGuiTxt", 10.0f);
	}


}


