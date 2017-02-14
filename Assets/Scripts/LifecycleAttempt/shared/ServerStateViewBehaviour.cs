using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;


public abstract class ServerStateViewBehaviour : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		ServerSceneManager.instance.stateChangeEvent += OnServerStateChange;
		OnServerStateChange (ServerSceneManager.instance.CurrentState ());
	}

	void OnDestroy () {
		ServerSceneManager.instance.stateChangeEvent -= OnServerStateChange;
	}

	protected abstract void OnServerStateChange (ProcessState state);


}


