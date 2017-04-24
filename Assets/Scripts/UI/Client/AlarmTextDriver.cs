using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmTextDriver : MonoBehaviour {

	public Text AlarmText;
	public Text LoadingText;

	void Start(){
		if (ClientSceneManager.Instance != null) {
			ClientSceneManager.Instance.OnCountDownTimeUpdateEvent += OnCountDownTimeUpdate;
		}
	}

	void OnDestroy(){
		if (ClientSceneManager.Instance != null) {
			ClientSceneManager.Instance.OnCountDownTimeUpdateEvent -= OnCountDownTimeUpdate;
		}
	}

	public void OnCountDownTimeUpdate (int remainingTime) {
		Debug.Log (string.Format ("{0}s remaining until game start", remainingTime));
		AlarmText.text = string.Format ("00:{0}", remainingTime.ToString ("00"));
		LoadingText.enabled = remainingTime == 0;
	}
		
}
