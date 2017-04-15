﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerDataManager {

	[Serializable]
	public class PlayerDataList
	{
		public PlayerData[] players;

		public PlayerDataList () {
			players = new PlayerData[0];
		}
	}

	[Serializable]
	public class PlayerData
	{
		public int ServerId;
		public int colour;
		public string Name;
		public int FinishPosition;
		public float RemainingTime;
	}

	public PlayerDataList list = new PlayerDataList();

	private GameLobbyManager _networkManager;
	private int _thisPlayerID;

	public PlayerDataManager (GameLobbyManager networkManager) {
		_networkManager = networkManager;
		_networkManager.OnUpdatePlayerDataEvent += OnListUpdate;
		_networkManager.OnPlayerIDEvent += OnPlayerID;
		_networkManager.OnLobbyClientNameSubmissionEvent += SetPlayerName;
	}

	private void OnPlayerID (int playerID) {
		_thisPlayerID = playerID;
	}

	private void OnListUpdate (string data) {
		
		list = JsonUtility.FromJson<PlayerDataList> (data);
	}
		
	private void InvalidateList () {
		_networkManager.UpdatePlayerData (JsonUtility.ToJson (list));
	}
		
	public void AddPlayer (int serverId, int colour) {
		// don't allow dupes
		foreach (var player in list.players) {
			if (player.ServerId == serverId)
				return;
		}

		PlayerData data = new PlayerData ();
		data.FinishPosition = -1;
		data.RemainingTime = -1;
		data.Name = null;
		data.colour = colour;
		data.ServerId = serverId;

		var playersList = new List<PlayerData> (list.players);
		playersList.Add (data);
		list.players = playersList.ToArray ();

		InvalidateList ();



	}
		
	public void RemovePlayer (int serverId) {
		for (int i = 0; i < list.players.Length; i++) {
			var player = list.players [i];
			if (player.ServerId == serverId) {
				var playersList = new List<PlayerData> (list.players);
				playersList.RemoveAt (i);
				list.players = playersList.ToArray ();
				InvalidateList ();
				return;
			}
		}
	}

	private void SetPlayerName (int serverId, string name) {
		Debug.Log (string.Format ("setting player id {0}'s name to {1}", serverId, name));
		GetPlayerById (serverId).Name = name;
		InvalidateList ();
	}

	public void UpdatePlayerGameData (int serverId, int finishPosition, float remainingTime) {		
		for (int i=0; i<list.players.Length; i++) {
			PlayerData player = list.players [i];
			if (player.ServerId == serverId) {
				player.FinishPosition = finishPosition;
				player.RemainingTime = remainingTime;
				InvalidateList ();
				return;
			}
		}
	}

	public bool HasGameData () {
		foreach (var player in list.players) {
			if (player.FinishPosition >= 0) {
				return true;
			}
		}
		return false;
	}
		
	public void ResetAllGameData () {
		for (int i = 0; i < list.players.Length; i++) {
			var player = list.players [i];
			player.FinishPosition = -1;
			player.RemainingTime = -1;
			InvalidateList ();
		}
	}

	public PlayerData GetPlayerById (int serverId) {
		foreach (var player in list.players) {
			if (player.ServerId == serverId) {
				return player;
			}
		}
		return null;
	}

	public PlayerData GetThisPlayer () {
		return GetPlayerById (_thisPlayerID);	
	}
		
}
