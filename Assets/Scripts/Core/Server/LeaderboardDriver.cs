﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LeaderboardDriver : MonoBehaviour {

	public GameObject LeaderboardEntryPrefab;

	void Start () {
		List<PlayerDataManager.PlayerData> SortedList = new List<PlayerDataManager.PlayerData> (ServerSceneManager.Instance.GetPlayerData()).OrderBy(o=>o.FinishPosition).ToList();

		foreach (var player in SortedList) {
			Debug.LogError (string.Format ("got player game data: id: {0}, name: {1}, finishPosition: {2}, remainingTime: {3}", player.ServerId, player.Name, player.FinishPosition, player.RemainingTime));
			var entry = GameObject.Instantiate (LeaderboardEntryPrefab);
			entry.transform.Find ("place").GetComponent<Text> ().text = (player.FinishPosition + 1).ToString();
			entry.transform.Find ("name").GetComponent<Text> ().text = player.Name;
			entry.transform.Find ("remaining").GetComponent<Text> ().text = string.Format("{0}s", player.RemainingTime.ToString("n2"));

			entry.transform.parent = transform;
		}
	}

}
