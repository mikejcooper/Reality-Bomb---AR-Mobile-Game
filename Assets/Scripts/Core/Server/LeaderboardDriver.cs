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
			entry.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = (player.lastGameResult.FinishPosition + 1).ToString();
			entry.transform.Find ("name").GetComponent<TextMeshProUGUI> ().text = player.nickname;
			entry.transform.Find ("remaining").GetComponent<TextMeshProUGUI> ().text = string.Format("{0}s", player.lastGameResult.FinishTime.ToString("n2"));

			entry.transform.parent = transform;
		}
	}

}
