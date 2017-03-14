using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SandboxManager : MonoBehaviour
{
	//The following code  has been taken from CarController
	/********* TODO: Change the following references to _rigidbody to a reference of the car ******/

	public GameObject CarObject;
	public GameObject PlaneObject;
	public PowerUpManager PowerUpManagerObject;

	private bool _controlsDisabled;
	private string _gui_Txt = "Welcome to the Tutorial!\nUse the joystick to drive around but don't fall off the map!\nSee if you can pickup some Power Ups along the way!";
	private string _splatter_Txt = "You've Activated the Splatter Power Up!\nThis splatters ink on your opponents' screens\n as shown above making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost Power Up!\nEnjoy double the speed but becareful not to lose control!";

	void Start(){
		_controlsDisabled = false;
		Invoke ("ClearGuiTxt", 10.0f);
		PowerUpManagerObject.OnSplatterStartEvent += Splat;
	}

	void Update(){
		//EnsureCarIsOnMap ();
	}

	void OnGUI(){
		GUI.Label(new Rect(Screen.width/2.0f - 150,Screen.height/40.0f, 800, 800), _gui_Txt);
	}


	private void ClearGuiTxt(){
		_gui_Txt = "";
	}

	private void Splat () {
		DebugConsole.Log ("Splat!");
	}

	/*
	public void EnsureCarIsOnMap(){
		if(CarObject.position.y <= - 30.0f){
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

		//Set velocities to zero
		CarObject.velocity = Vector3.zero;
		CarObject.angularVelocity = Vector3.zero;

		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);

		if (position != Vector3.zero) {
			DebugConsole.Log ("unfreezing");
			// now unfreeze and show

			gameObject.SetActive (true);
			CarObject.isKinematic = false;

			CarObject.position = position;
		}

	}
	*/



}


