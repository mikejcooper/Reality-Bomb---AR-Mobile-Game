using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameLobbyManager : NetworkLobbyManager {
    private int players = 0;

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		players++;
		Debug.Log("Players = " + players);
		if (players == 2)
		{
			gamePlayer.gameObject.GetComponent<TankController>().hasBomb = true;
		}
		return true;
	}
}
