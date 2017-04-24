using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Powerups;

public class GameMessageManager : NetworkBehaviour {

	public PreparingGame PreparingCanvas;
	public CanvasMessages CanvasMessage;
	public GameObject GameStatusObject;


	// Use this for initialization
	void Start () {
		if (isServer) {
			if (GameObject.FindObjectOfType<Joystick> ()) {
				GameObject.FindObjectOfType<Joystick> ().gameObject.SetActive (false);
			}
			if (GameObject.Find ("HealthBar") != null) {
				GameObject.Find ("HealthBar").SetActive (false);
			}
			if (GameObject.Find ("MarkerAlert") != null) {
				GameObject.Find ("MarkerAlert").SetActive (false);
			}
			if (GameObject.Find ("SpectatingText") != null) {
				GameObject.Find ("SpectatingText").GetComponent<TextMeshProUGUI> ().text = "Spectating...";
			}
		} else {
			GameObject.FindObjectOfType<GameManager> ().GameCountDownFinishedEvent += HideGameStatus;
		}

    }

	public void SetSplatTxt(){
		CanvasMessage.DisplayPowerUpMessage ("Ink Splatter");
	}

	public void SetSpeedTxt(){
		CanvasMessage.DisplayPowerUpMessage ("Speed Boost");
	}

    public void SetShieldTxt()
    {
        CanvasMessage.DisplayPowerUpMessage("Shield");
    }

    public void SetRespawnTxt(){
	}

	public void HideGameStatus(){
		GameStatusObject.SetActive (false);
	}
		
}
