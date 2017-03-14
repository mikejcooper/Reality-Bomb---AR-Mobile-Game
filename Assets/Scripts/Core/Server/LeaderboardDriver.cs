using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class LeaderboardDriver : MonoBehaviour {

	public GameObject LeaderboardEntryPrefab;

	void Start () {
		List<PlayerDataManager.PlayerData> SortedList = new List<PlayerDataManager.PlayerData> (ServerSceneManager.Instance.GetPlayerData()).OrderBy(o=>o.FinishPosition).ToList();

		foreach (var player in SortedList) {
			var entry = GameObject.Instantiate (LeaderboardEntryPrefab);
			entry.transform.Find ("place").GetComponent<TextMeshProUGUI> ().text = (player.FinishPosition + 1).ToString();
			entry.transform.Find ("name").GetComponent<TextMeshProUGUI> ().text = player.Name;
			entry.transform.Find ("remaining").GetComponent<TextMeshProUGUI> ().text = string.Format("{0}s", player.RemainingTime.ToString("n2"));

			entry.transform.parent = transform;
		}
	}

}
