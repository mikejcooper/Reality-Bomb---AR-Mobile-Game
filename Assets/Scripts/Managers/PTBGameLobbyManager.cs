using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameLobbyManager : NetworkLobbyManager {
    private int players = 0;
	private int bombPlayer = -1;

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
	{
		if (bombPlayer == -1) 
		{
			bombPlayer = numPlayers - 1;
		} 
		Transform startPos = GetStartPosition();
		var gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
		
		if (players == bombPlayer)
		{
			gamePlayer.gameObject.GetComponent<TankController>().hasBomb = true;
		}
		players++;
		return gamePlayer;
	}

}
