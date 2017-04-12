using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class LeaderboardManager : MonoBehaviour
{
	public Canvas CanvasObj;
	public GameObject GameCountdownDialogPrefab;

	private GameObject _countdownDialog;

	void Start(){
		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent += OnCountDownTimeUpdate;
		ClientSceneManager.Instance.OnCountDownCanceledEvent += OnCountDownCanceled;
	}

	void OnDestroy(){
		ClientSceneManager.Instance.OnCountDownTimeUpdateEvent -= OnCountDownTimeUpdate;
		ClientSceneManager.Instance.OnCountDownCanceledEvent -= OnCountDownCanceled;
	}

	private void OnCountDownTimeUpdate (int remainingTime) {
		if (_countdownDialog == null) {
			_countdownDialog = GameObject.Instantiate (GameCountdownDialogPrefab);
			_countdownDialog.transform.SetParent (CanvasObj.transform, false);
		}
	}

	private void OnCountDownCanceled (string reason) {
		if (_countdownDialog != null) {
			Destroy (_countdownDialog);
			_countdownDialog = null;
		}
	}


}


