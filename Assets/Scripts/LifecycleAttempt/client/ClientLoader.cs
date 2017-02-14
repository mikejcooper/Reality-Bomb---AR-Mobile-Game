using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientLoader : MonoBehaviour {

	public ClientSceneManager clientSceneManager;

	void Awake () {
		if (ClientSceneManager.instance == null)
			Instantiate (clientSceneManager);
	}
	

}
