using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientLeaderboardGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable playMinigameButton;
	public UnityEngine.UI.Selectable leaveButton;

	void Start () {
		playMinigameButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ClientSceneManager.instance.onUserPlayMinigame());
		leaveButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ClientSceneManager.instance.onUserLeaveGame());
	}

}
