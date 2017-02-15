using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientIdleGUI : MonoBehaviour {

	public UnityEngine.UI.Selectable playNowButton;

	void Start () {
		playNowButton.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener(() => ClientSceneManager.Instance.onUserJoinGame());
	}

}
