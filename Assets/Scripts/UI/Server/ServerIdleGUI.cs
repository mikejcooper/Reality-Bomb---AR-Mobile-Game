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
		instantStartGameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.Instance.OnServerRequestGameStart(0));
		loadMeshButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.Instance.OnServerRequestLoadNewMesh());
	}

}
