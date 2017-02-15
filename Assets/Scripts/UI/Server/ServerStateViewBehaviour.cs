using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;


public abstract class ServerStateViewBehaviour : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		ServerSceneManager.Instance.StateChangeEvent += OnServerStateChange;
		OnServerStateChange (ServerSceneManager.Instance.CurrentState ());
	}

	void OnDestroy () {
		ServerSceneManager.Instance.StateChangeEvent -= OnServerStateChange;
	}

	protected abstract void OnServerStateChange (ProcessState state);


}


