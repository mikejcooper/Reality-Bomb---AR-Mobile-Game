using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmTextDriver : MonoBehaviour {

	public Text text;

	void Start(){
		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent += OnCountDownTimeUpdate;
	}

	void OnDestroy(){
		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent -= OnCountDownTimeUpdate;
	}

	private void OnCountDownTimeUpdate (int remainingTime) {
		Debug.Log (string.Format ("{0}s remaining until game start", remainingTime));
		text.text = string.Format ("00:{0}", remainingTime.ToString ("00"));
	}
		
}
