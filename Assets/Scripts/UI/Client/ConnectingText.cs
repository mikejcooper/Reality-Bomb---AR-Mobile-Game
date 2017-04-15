using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectingText : MonoBehaviour {


	private Text _textComponent;
	private int _numberOfDots = 0;
	private float _blinkRate = 5.0f;

	// Use this for initialization
	void Start () {
		_textComponent = GetComponent<Text> ();
	}

	public void StartBlinking() {
		InvokeRepeating ("Blink", 0.0f, 1 / _blinkRate);
	}

	public void StopBlinking () {
		CancelInvoke ();
		if (_textComponent != null) {
			_textComponent.text = "";
		}
		_numberOfDots = 0;
	}
	
	public void Blink() {
		string newText = "connecting";
		for (int i = 0; i < _numberOfDots; i++) {
			newText += ".";
		}
		_textComponent.text = newText;
		_numberOfDots = (_numberOfDots + 1) % 3;
	}
}
