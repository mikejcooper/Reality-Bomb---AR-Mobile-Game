using System;
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
		public string Name;
		public int FinishPosition;
		public float RemainingTime;
	}

	public PlayerDataList list = new PlayerDataList();

	private GameLobbyManager _networkManager;


	public PlayerDataManager (GameLobbyManager networkManager) {
		_networkManager = networkManager;
		_networkManager.OnUpdatePlayerDataEvent += OnListUpdate;
	}

	private void OnListUpdate (string data) {
		
		list = JsonUtility.FromJson<PlayerDataList> (data);
		Debug.LogError (string.Format("received OnListUpdate list length: {0}", list.players.Length));
	}
		
	private void InvalidateList () {
		Debug.LogError ("data has been invalidated");
		_networkManager.UpdatePlayerData (JsonUtility.ToJson (list));
	}
		
	public void AddPlayer (int serverId, string name) {
		Debug.LogError (string.Format ("adding player: id: {0}, name: {1}", serverId, name));
		// don't allow dupes
		foreach (var player in list.players) {
			if (player.ServerId == serverId)
				return;
		}

		PlayerData data = new PlayerData ();
		data.FinishPosition = -1;
		data.RemainingTime = -1;
		data.Name = name;
		data.ServerId = serverId;

		var playersList = new List<PlayerData> (list.players);
		playersList.Add (data);
		list.players = playersList.ToArray ();

		InvalidateList ();



	}
		
	public void RemovePlayer (int serverId) {
		Debug.LogError (string.Format ("removing player: id: {0}", serverId));
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

	public void UpdatePlayerGameData (int serverId, int finishPosition, float remainingTime) {
		Debug.LogError (string.Format ("updating player game data: id: {0}, finishPosition: {1}, remainingTime: {2}", serverId, finishPosition, remainingTime));
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
		Debug.LogError ("returning HasGameData: false");
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

	public PlayerData getPlayerById (int serverId) {
		Debug.LogError (string.Format ("getting player with id: {0}, ", serverId));
		foreach (var player in list.players) {
			if (player.ServerId == serverId) {
				Debug.LogError (string.Format ("found player with id: {0}, ", serverId));
				return player;
			}
		}
		Debug.LogError (string.Format ("didn't find player with id: {0}, list size: {1}", serverId, list.players.Length));
		return new PlayerData();
	}
		
}
