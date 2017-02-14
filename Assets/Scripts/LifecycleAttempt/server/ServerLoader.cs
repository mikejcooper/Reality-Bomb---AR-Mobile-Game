using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerLoader : MonoBehaviour {

	public ServerSceneManager serverSceneManager;

	void Awake () {
		if (ServerSceneManager.instance == null)
			Instantiate (serverSceneManager);
	}
	

}
