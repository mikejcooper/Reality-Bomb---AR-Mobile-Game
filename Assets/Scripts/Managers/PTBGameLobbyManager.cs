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
        //Transform startPos =  GetStartPosition();
		var gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
        //gamePlayer.GetComponent<Renderer>().enabled = false;
		if (players == bombPlayer)
		{
			gamePlayer.gameObject.GetComponent<CarController>().hasBomb = true;
		}
		players++;
		return gamePlayer;
	}

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        PTBGameManager.AddCar(gamePlayer);
        return true;
    }

}
