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

//	[ClientRpc]
//	public void spawnPlayer(GameObject mesh) {
//		tankObject.transform = mesh.transform;
//		tankObject.transform.position.y += mesh.GetComponent<MeshRenderer> ().bounds.size.y/2;
//		// use the tank references to spawn to some location
//
//		// use center of mesh with random
//
//		// show
//		tankObject.SetActive (true);
//	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
//		var pos = GetStartPosition ();
//		Debug.Log (pos);

		tankObject = (GameObject)GameObject.Instantiate (playerPrefab, new Vector3(0,0,0), Quaternion.identity);
		

        players++;
        Debug.Log("Players = " + players);
        if (players == 2)
        {
			tankObject.gameObject.GetComponent<TankController>().hasBomb = true;
        }
		NetworkServer.AddPlayerForConnection(conn, tankObject, playerControllerId);

		// hides the tank when its spawned
		tankObject.SetActive (false);


    }
}
