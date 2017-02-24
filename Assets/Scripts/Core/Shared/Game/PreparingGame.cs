using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;


public class PreparingGame : MonoBehaviour
{

	public Sprite WaitingForPlayersToConnect;
	public Sprite Three;
	public Sprite Two;
	public Sprite One;
	public Sprite GO;

	private GameObject _waitingForPlayersToConnectObj;
	private GameObject _threeObj;
	private GameObject _twoObj;
	private GameObject _oneObj;
	private GameObject _goObj;
//
//
//


	void Start () {
		_waitingForPlayersToConnectObj = CreateSprite (WaitingForPlayersToConnect);
		_threeObj = CreateSprite (Three);
		_twoObj = CreateSprite (Two);
		_oneObj = CreateSprite (One);
		_goObj = CreateSprite (GO);
		_waitingForPlayersToConnectObj.SetActive (true);
		StartGameCountDown ();
	}
		
	IEnumerator StartCountDown() {
		float t1 = 0.3f; // time hide and show
		float t2 = 0.8f; // time image is shown
//		WaitingForPlayersToConnectObj.SetActive (true);
//		yield return new WaitForSeconds (t2);
//		WaitingForPlayersToConnectObj.SetActive (false);

		yield return new WaitForSeconds (t1);
		_threeObj.SetActive (true);
		yield return new WaitForSeconds (t2);
		_threeObj.SetActive (false);
		yield return new WaitForSeconds (t1);
		_twoObj.SetActive (true);
		yield return new WaitForSeconds (t2);
		_twoObj.SetActive (false);
		yield return new WaitForSeconds (t1);
		_oneObj.SetActive (true);
		yield return new WaitForSeconds (t2);
		_oneObj.SetActive (false);
		yield return new WaitForSeconds (t1);		
		_goObj.SetActive (true);
		yield return new WaitForSeconds (t2);
		_goObj.SetActive (false);
	}

	public void StartGameCountDown(){
		_waitingForPlayersToConnectObj.SetActive (false);
		StartCoroutine(StartCountDown ());
	}
		

	private GameObject CreateSprite (Sprite sprite) {
		GameObject obj = new GameObject();
		Image image = obj.AddComponent<Image> ();
		RectTransform rectTransform = obj.GetComponent<RectTransform> ();

		image.sprite = sprite;

		RectTransform canvasRect = GetComponent<RectTransform>();
		rectTransform.parent = canvasRect.transform;
		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one;

		obj.SetActive (false);
		return obj;
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
