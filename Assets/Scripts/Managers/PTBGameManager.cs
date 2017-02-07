using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkManager {

	private bool s_root;
    private int players = 0;
	private GameObject tankObject;

	private void Awake(){
		s_root = GameObject.Find("Scene Root").gameObject.GetComponent<DataTransferManager>();
	}

	private void Update(){
		
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		tankObject = (GameObject)GameObject.Instantiate (playerPrefab, new Vector3(0,0,0), Quaternion.identity);
		

        players++;
        Debug.Log("Players = " + players);
        if (players == 2)
        {
			tankObject.gameObject.GetComponent<TankController>().hasBomb = true;
        }
		NetworkServer.AddPlayerForConnection(conn, tankObject, playerControllerId);


    }
}
