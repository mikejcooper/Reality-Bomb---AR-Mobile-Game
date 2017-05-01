using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class LeaderboardDriver : MonoBehaviour {

	public GameObject LeaderboardEntryPrefab;

	void Start () {
		List<NetworkCompat.NetworkLobbyPlayer> SortedList = GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ().OrderBy (player => {
			return player.lastGameResult.FinishPosition;
		}).ToList ();

		foreach (var player in SortedList) {
			var entry = GameObject.Instantiate (LeaderboardEntryPrefab);
			var finishPositionStr = (player.lastGameResult.FinishPosition + 1).ToString ();
			var lastRoundScore = player.lastGameResult.TotalPlayers - player.lastGameResult.FinishPosition - 1;
			string lastRoundScoreStr;
			if (lastRoundScore > 0) {
				lastRoundScoreStr = "+"+lastRoundScore.ToString ();
			} else {
				lastRoundScoreStr = "";
			}
			var cumulativeScoreStr = player.totalCumulativeGamesScore ().ToString ();
			entry.transform.Find ("Pos").GetComponent<Text> ().text = finishPositionStr;
			entry.transform.Find ("Nickname").GetComponent<Text> ().text = player.nickname;
			entry.transform.Find ("RoundScore").GetComponent<Text> ().text = lastRoundScoreStr;
			entry.transform.Find ("CumulativeScore").GetComponent<Text> ().text = cumulativeScoreStr;

			entry.transform.SetParent (transform, false);
		}

		if (SortedList.Count == 0) {
			for (int i = 0; i < 3; i++) {
				var entry = GameObject.Instantiate (LeaderboardEntryPrefab);
				var finishPositionStr = (i + 1).ToString ();
				var lastRoundScoreStr = "+" + (10).ToString ();
				var cumulativeScoreStr = 12.ToString ();
				entry.transform.Find ("Pos").GetComponent<Text> ().text = finishPositionStr;
				entry.transform.Find ("Nickname").GetComponent<Text> ().text = "Roarster";
				entry.transform.Find ("RoundScore").GetComponent<Text> ().text = lastRoundScoreStr;
				entry.transform.Find ("CumulativeScore").GetComponent<Text> ().text = cumulativeScoreStr;

				entry.transform.SetParent (transform, false);
			}
		}
	}

}
