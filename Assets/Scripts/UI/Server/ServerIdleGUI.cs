using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerLifecycle;

public class ServerIdleGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable startGameButton;
	public UnityEngine.UI.Selectable instantStartGameButton;
	public UnityEngine.UI.Selectable loadMeshButton;

	void Start () {
		startGameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => { 
			switch (ServerSceneManager.Instance.CurrentState()) {
			case ProcessState.PreparingGame:
				ServerSceneManager.Instance.OnServerRequestGameStart (5);
				break;
			case ProcessState.CountingDown:
				ServerSceneManager.Instance.OnServerRequestCancelGameStart ();
				break;
			}

		});
		instantStartGameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(OnInstantStart);
		loadMeshButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(OnLoadMesh);
	}

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
			var muteButton = GetComponentInChildren<MuteButton> ();
			if (muteButton != null) {
				muteButton.ToggleMuteSound ();
			} else {
				Debug.LogWarning ("could not find mute button");
			}
		}

	}

}
