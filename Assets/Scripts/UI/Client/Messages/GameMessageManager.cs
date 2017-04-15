using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Powerups;

public class GameMessageManager : NetworkBehaviour {

	public PreparingGame PreparingCanvas;
	public CanvasMessages CanvasMessage;

	// Use this for initialization
	void Start () {
		if (isServer) {
			if (GameObject.FindObjectOfType<Joystick> ()) {
				GameObject.FindObjectOfType<Joystick> ().gameObject.SetActive (false);
			}
			if (GameObject.Find ("HealthBar") != null) {
				GameObject.Find ("HealthBar").SetActive (false);
			}
			if (GameObject.Find("MarkerAlert") != null)
			{
				GameObject.Find("MarkerAlert").SetActive(false);
			}
			if (GameObject.Find ("SpectatingText") != null) {
				GameObject.Find ("SpectatingText").GetComponent<TextMeshProUGUI>().text = "Spectating...";
			}
		}
		BasePowerUpManager.SpeedBoostActivatedEvent += SetSpeedTxt;
		BasePowerUpManager.InkSplatterActivatedEvent += SetSplatTxt;
        BasePowerUpManager.ShieldActivatedEvent += SetShieldTxt;
    }

	void OnDestroy(){
		BasePowerUpManager.SpeedBoostActivatedEvent -= SetSpeedTxt;
		BasePowerUpManager.InkSplatterActivatedEvent -= SetSplatTxt;
        BasePowerUpManager.ShieldActivatedEvent -= SetShieldTxt;
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
		
}
