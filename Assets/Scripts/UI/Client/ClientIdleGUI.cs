using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientIdleGUI : MonoBehaviour {

	public Button PlayNowButton;
	public Button CancelButton;

	public Button JoinGameButton;

	public GameObject SceneObject;

	public GameObject NickNameObject;
	public GameObject FormObject;

	public InputField NickNameText;
	public Text ErrorText;
	public ConnectingText ConnectingTextObject;

	private ProfanityFilter _filter;

	private string[] _profanityResponses = new string[] {
		"does your mother know you speak like that?",
		"wash your mouth out with soap!",
		"let's try another name"
	};

	void Start () {
		_filter = new ProfanityFilter ();

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
			SceneObject.SetActive(false);
		});

		CancelButton.onClick.AddListener (() => {
			if (ClientSceneManager.Instance.CurrentState() != ClientLifecycle.ProcessState.Idle) {
				ClientSceneManager.Instance.OnUserRequestLeaveGame();
			}
			NickNameObject.SetActive(false);
			SceneObject.SetActive(true);
		});
	}

	private string validateNickName() {

		string nickName = NickNameText.text;

		// check for zero length
		if (nickName.Trim ().Length == 0) {
			return "please enter a nickname";
		}

		if (!_filter.IsClean (nickName)) {
			NickNameText.text = "";
			return _profanityResponses [new System.Random ().Next (_profanityResponses.Length - 1)];
		}

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
