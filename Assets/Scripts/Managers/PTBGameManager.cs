using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkManager {
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		var pos = GetStartPosition ();
		Debug.Log (pos);
		var player = (GameObject)GameObject.Instantiate (playerPrefab, pos.position, Quaternion.identity);
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
	}
}
