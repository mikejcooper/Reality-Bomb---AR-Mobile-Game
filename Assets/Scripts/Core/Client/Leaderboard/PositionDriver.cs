using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PositionDriver : MonoBehaviour {

	public TextMeshProUGUI PositionText;
	public TextMeshProUGUI OrdinalText;

	void Start () {
//		if (ClientSceneManager.Instance == null || ClientSceneManager.Instance.GetThisPlayerData () == null) {
//			Debug.LogWarning ("making up our own player data because this client's player data is null");
//			DisplayFinishPosition(new System.Random().Next(10));
//		} else {
//		DisplayFinishPosition (ClientSceneManager.Instance.GetThisPlayerData ().FinishPosition);
//		}

		NetworkCompat.NetworkLobbyPlayer thisLobbyPlayer = null;
		foreach (var lobbyPlayer in GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ()) {
			if (lobbyPlayer.isLocalPlayer) {
				thisLobbyPlayer = lobbyPlayer;
				break;
			}
		}

		if (thisLobbyPlayer == null) {
			return;
		}

		DisplayFinishPosition (thisLobbyPlayer.lastGameResult.FinishPosition);
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
