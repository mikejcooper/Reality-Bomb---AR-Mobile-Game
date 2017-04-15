using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientIdleGUI : MonoBehaviour {

	public Button PlayNowButton;
	public Button CancelButton;

	public Button JoinGameButton;

	public GameObject NickNameObject;
	public GameObject FormObject;

	public InputField NickNameText;
	public Text ErrorText;
	public ConnectingText ConnectingTextObject;

	void Start () {

		JoinGameButton.onClick.AddListener (() => {
			string errorString = validateNickName();
			if (errorString != null) {
				displayErrorText(errorString);
			} else {
				displayErrorText(null);
				ConnectingTextObject.StartBlinking();
				FormObject.SetActive(false);
				ClientSceneManager.Instance.OnUserRequestFindGame(NickNameText.text);
			}
		});

		PlayNowButton.onClick.AddListener(() => {
			resetNickNameForm();
			NickNameObject.SetActive(true);
		});

		CancelButton.onClick.AddListener (() => {
			if (ClientSceneManager.Instance.CurrentState() != ClientLifecycle.ProcessState.Idle) {
				ClientSceneManager.Instance.OnUserRequestLeaveGame();
			}
			NickNameObject.SetActive(false);
		});
	}

	private string validateNickName() {

		string nickName = NickNameText.text;

		// check for zero length
		if (nickName.Trim ().Length == 0) {
			return "please enter a nickname";
		}

		// todo: check for inappropriate names

		// possible todo: check server for taken names

		return null;
	}

	private void resetNickNameForm() {
		ErrorText.text = "";
		NickNameText.text = "";
		FormObject.SetActive (true);
		ConnectingTextObject.StopBlinking();
	}

	private void displayErrorText (string errorString) {
		ErrorText.text = errorString;
	}

}
