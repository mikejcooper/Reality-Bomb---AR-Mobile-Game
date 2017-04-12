using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinButton : MonoBehaviour {

	private Text _textComponent;

	// Use this for initialization
	void Start () {
		_textComponent = GetComponentInChildren<Text> ();
	}

	void ChangeToCancelText () {
		_textComponent.text = "Cancel";
	}

}
