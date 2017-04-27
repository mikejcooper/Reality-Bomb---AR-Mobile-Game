using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientIdleGUI : MonoBehaviour {

	public Button PlayNowButton;

	void Start () {
		PlayNowButton.onClick.AddListener(() => {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("Login");
		});
	}
}
