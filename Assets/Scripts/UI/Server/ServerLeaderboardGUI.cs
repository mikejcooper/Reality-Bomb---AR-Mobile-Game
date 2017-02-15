using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerLeaderboardGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable startGameButton;
	public UnityEngine.UI.Selectable loadMeshButton;

	void Start () {
		startGameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.Instance.OnServerRequestGameReady());
		loadMeshButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ServerSceneManager.Instance.OnServerRequestLoadNewMesh());
	}

}
