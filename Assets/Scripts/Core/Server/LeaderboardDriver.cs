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
			if (player.gameResults.Count > 0) {
				return player.lastGameResult.FinishPosition;
			} else {
				return 1;
			}
		}).ToList ();

		foreach (var player in SortedList) {
			var entry = GameObject.Instantiate (LeaderboardEntryPrefab);

			string finishPositionStr, lastRoundScoreStr, cumulativeScoreStr;
			if (player.gameResults.Count > 0) {
				finishPositionStr = (player.lastGameResult.FinishPosition + 1).ToString ();
				var lastRoundScore = player.lastGameResult.TotalPlayers - player.lastGameResult.FinishPosition - 1;

				if (lastRoundScore > 0) {
					lastRoundScoreStr = "+" + lastRoundScore.ToString ();
				} else {
					lastRoundScoreStr = "";
				}
			} else {
				finishPositionStr = "1";
				lastRoundScoreStr = "";
			}

			cumulativeScoreStr = player.totalCumulativeGamesScore ().ToString ();

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
