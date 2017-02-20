using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientLeaderboardGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable playSandboxButton;
	public UnityEngine.UI.Selectable leaveButton;

	void Start () {
		playSandboxButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ClientSceneManager.Instance.OnUserRequestPlaySandbox());
		leaveButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ClientSceneManager.Instance.OnUserRequestLeaveGame());
	}

}
