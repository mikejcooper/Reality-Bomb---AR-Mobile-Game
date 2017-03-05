using System.Collections;
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

	void Start () {
		_entry = GameObject.Instantiate (LeaderboardEntryPrefab);
		_entry.transform.Find ("Waiting").GetComponent<TextMeshProUGUI> ().text = "Waiting for players...";
		_entry.transform.parent = transform;
		StartCoroutine(StartCountDown ());
	}

	IEnumerator StartCountDown() {
		float t1 = 1.2f; // time hide and show
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Waiting").GetComponent<TextMeshProUGUI> ().text = "";
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Numbers").GetComponent<TextMeshProUGUI> ().text = "3";
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Numbers").GetComponent<TextMeshProUGUI> ().text = "2";
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Numbers").GetComponent<TextMeshProUGUI> ().text = "1";
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Numbers").GetComponent<TextMeshProUGUI> ().text = "";
		_entry.transform.Find ("Go").GetComponent<TextMeshProUGUI> ().text = "Go!";
		yield return new WaitForSeconds (t1);
		_entry.transform.Find ("Go").GetComponent<TextMeshProUGUI> ().text = "";
		if(CountDownFinishedEvent != null)
			CountDownFinishedEvent ();
	}

	public void StartGameCountDown(){
		_entry.transform.Find ("Waiting").GetComponent<TextMeshProUGUI> ().text = "";
		StartCoroutine(StartCountDown ());
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
