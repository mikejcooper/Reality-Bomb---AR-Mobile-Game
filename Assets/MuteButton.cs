using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour {

	public bool Muted;
	private Text _buttonText;

	void Start() {
		DontDestroyOnLoad (transform.parent.gameObject);
		_buttonText = GetComponentInChildren<Text> ();
		if (Muted == true) {
			Muted = false;
			ToggleMuteAllSound();
		}
	}

	// Toggle all sound on and off
	public void ToggleMuteAllSound() {
		if (Muted) {
			AudioListener.volume = 1.0f;
			_buttonText.text = "Mute";
		} else {
			AudioListener.volume = 0.0f;
			_buttonText.text = "Unmute";
		}
		Muted = !Muted;
	}
}
