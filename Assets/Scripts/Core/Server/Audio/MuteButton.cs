using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour {
	
	private Text _buttonText;

	void Start() {
		_buttonText = GetComponentInChildren<Text>();
		if(AudioListener.volume == 0.0f) {
			_buttonText.text = "Unmute (m)";
		}
	}

	//Toggle all sound on and off
	public void ToggleMuteSound() {
		if(AudioListener.volume == 0.0f) {
			AudioListener.volume = 1.0f;
			_buttonText.text = "Mute (m)";
		}
		else {
			AudioListener.volume = 0.0f;
			_buttonText.text = "Unmute (m)";
		}
	}
}