using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;


public abstract class ServerStateViewBehaviour : MonoBehaviour {
	
	public GameObject persistentObject;
	protected ServerSceneManager serverSceneManager;

	// Use this for initialization
	void Start () {
		serverSceneManager = persistentObject.GetComponent<ServerSceneManager> ();
		serverSceneManager.stateChangeEvent += new ServerSceneManager.StateChangeCallback (OnServerStateChange);
		OnServerStateChange (serverSceneManager.CurrentState ());
	}

	protected abstract void OnServerStateChange (ProcessState state);


}


