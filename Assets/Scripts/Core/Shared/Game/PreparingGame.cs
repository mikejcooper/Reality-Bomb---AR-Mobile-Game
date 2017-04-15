﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;
using TMPro;



public class PreparingGame : MonoBehaviour
{
	public delegate void CountDownFinished();
	public event CountDownFinished CountDownFinishedEvent;  

	public GameObject LeaderboardEntryPrefab;

	private GameObject _entry;

	public AudioSource _beep;
	public AudioSource _boop;

	void Start () {
		_entry = GameObject.Instantiate (LeaderboardEntryPrefab);
//		_entry.transform.Find ("Waiting").GetComponent<TextMeshProUGUI> ().text = "Waiting for players...";
		_entry.transform.parent = transform;
//		GameManager.StartGameCountDownEvent += StartGameCountDown ();
	}

	IEnumerator StartCountDown(bool sound) {
		float t1 = 1.2f; // time shown
		float t2 = 0.3f; // time between shown
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Waiting").GetComponent<TextMeshProUGUI> ().text = "";

		yield return StartCoroutine(ShowHideText (t1, t2, "3", "Numbers", sound, _beep));
		yield return StartCoroutine(ShowHideText (t1, t2, "2", "Numbers", sound, _beep));
		yield return StartCoroutine(ShowHideText (t1, t2, "1", "Numbers", sound, _beep));
		yield return StartCoroutine(ShowHideText (t1, t2, "Go!", "Go", sound, _boop));
		if(CountDownFinishedEvent != null)
			CountDownFinishedEvent ();
	}

	IEnumerator ShowHideText(float t1, float t2, string text, string component, bool sound, AudioSource soundClip){
		yield return new WaitForSeconds (t2);
		_entry.transform.Find (component).GetComponent<TextMeshProUGUI> ().text = text;
		//Play the sound clip
		if (sound && soundClip != null) {
			soundClip.Play ();
		}
		yield return new WaitForSeconds (t1);
		_entry.transform.Find (component).GetComponent<TextMeshProUGUI> ().text = "";
	}
		

	public void StartGameCountDown(bool sound){
		StartCoroutine(StartCountDown (sound));
	}

	public void ShowArrowOnCurrentPlayer(){
		// make arrow appear
	}	

	public void HideArrowOnCurrentPlayer(){
	}



	private void PlayNumberSound(){
		// Trigger sound
	}

	private void PlayGoSound(){
		// Trigger sound 
	}
}
