using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerLeaderboardGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable startGameButton;
	public UnityEngine.UI.Selectable loadMeshButton;

	void Start () {
		startGameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.instance.onGameReady());
		loadMeshButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.instance.loadNewMesh());
	}

}
