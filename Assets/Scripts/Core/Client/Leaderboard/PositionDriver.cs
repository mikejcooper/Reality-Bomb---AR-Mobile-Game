using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionDriver : MonoBehaviour {

	public Text PositionText;
	public Text OrdinalText;

	void Start () {
		if (ClientSceneManager.Instance == null || ClientSceneManager.Instance.GetThisPlayerData () == null) {
			Debug.LogWarning ("making up our own player data because this client's player data is null");
			DisplayFinishPosition(new System.Random().Next(10));
		} else {
			DisplayFinishPosition (ClientSceneManager.Instance.GetThisPlayerData ().FinishPosition);
		}
	}
	
	private void DisplayFinishPosition (int position) {
		int correctedPosition = position + 1;
		PositionText.text = correctedPosition.ToString ();
		OrdinalText.text = GetOrdinal (correctedPosition);
	}

	private string GetOrdinal (int position) {
		int tenRemainder = position % 10;
		switch (tenRemainder) {
		case 1:
			return "st";
		case 2:
			return "nd";
		case 3:
			return "rd";
		default:
			return "th";
		}
	}
}
