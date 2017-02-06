using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkManager {
    private int players = 0;
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		var pos = GetStartPosition ();
		//Debug.Log (pos);
		var player = (GameObject)GameObject.Instantiate (playerPrefab, pos.position, Quaternion.identity);
		

        players++;
        Debug.Log("Players = " + players);
        if (players == 2)
        {
            player.gameObject.GetComponent<TankController>().hasBomb = true;
        }
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
