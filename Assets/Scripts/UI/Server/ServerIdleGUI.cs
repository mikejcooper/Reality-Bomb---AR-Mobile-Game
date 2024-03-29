﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;

public class ServerIdleGUI : MonoBehaviour {
	
	private void OnInstantStart () {
		ServerSceneManager.Instance.OnServerRequestGameStart (0);
	}

	private void OnLoadMesh () {
		ServerSceneManager.Instance.OnServerRequestLoadNewMesh ();
	}

	void OnGUI() {
		
		if (Event.current.Equals (Event.KeyboardEvent ("s")) && ServerSceneManager.Instance.CurrentState() == ProcessState.PreparingGame)
			ServerSceneManager.Instance.OnServerRequestGameStart (5);

		if (Event.current.Equals (Event.KeyboardEvent ("c")) && ServerSceneManager.Instance.CurrentState() == ProcessState.CountingDown)
			ServerSceneManager.Instance.OnServerRequestCancelGameStart ();

		if (Event.current.Equals (Event.KeyboardEvent ("l")))
			OnLoadMesh ();

		if (Event.current.Equals (Event.KeyboardEvent ("i")))
			OnInstantStart ();

		if (Event.current.Equals (Event.KeyboardEvent ("m"))) {
			if(AudioListener.volume == 0.0f) {
				AudioListener.volume = 1.0f;
			}
			else {
				AudioListener.volume = 0.0f;
			}
		}

	}

}
