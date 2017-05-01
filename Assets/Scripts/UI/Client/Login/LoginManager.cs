using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour {

	public Button CancelButtonObj;
	public Button NextStageButton;
	public GameObject NickNameStageObj;
	public InputField NickNameText;
	public Text NickNameErrorText;
	public GameObject VehicleSelectionStageObj;
	public GameObject ConnectingStageObj;
	public VehiclePicker CarPickerObj;
	public ConnectingText ConnectingTextObject;
	public Image FadePanelObj;

	enum Stage { Nickname, Vehicle, Connecting };

	private Stage _currentStage = Stage.Nickname;
	private ProfanityFilter _profanityFilter;
	private string _chosenNickname;
	private Garage.Vehicle _chosenVehicle;

	private string[] _profanityResponses = new string[] {
		"does your mother know you speak like that?",
		"wash your mouth out with soap!",
		"let's try another name"
	};

	private bool fadingOut = false;
	delegate void FadeOutFinish ();
	private FadeOutFinish _fadeOutFinish;

	void Start () {
		CancelButtonObj.onClick.AddListener (OnCancel);
		NextStageButton.onClick.AddListener (OnNextStage);

		_profanityFilter = new ProfanityFilter ();

		NickNameStageObj.SetActive (true);
		VehicleSelectionStageObj.SetActive (false);
		ConnectingTextObject.gameObject.SetActive (false);

		var color = FadePanelObj.color;
		color.a = 1.0f;
		FadePanelObj.color = color;
	}

	private void OnCancel () {
		UnityEngine.SceneManagement.SceneManager.LoadScene ("Idle");
	}

	private void OnNextStage () {
		switch (_currentStage) {
		case Stage.Nickname:
			if (NickNameText.text.Trim ().Length == 0) {
				DisplayNicknameError ("please enter a nickname");
			} else if (!_profanityFilter.IsClean (NickNameText.text)) {
				DisplayNicknameError (_profanityResponses [new System.Random ().Next (_profanityResponses.Length - 1)]);
			} else {
				fadingOut = true;
				_fadeOutFinish = MoveToVehicleSelection;
			}
			break;
		case Stage.Vehicle:
			fadingOut = true;
			_fadeOutFinish = MoveToConnecting;
			break;
		}
	}

	private void DisplayNicknameError(string msg) {
		NickNameText.text = "";
		NickNameErrorText.text = msg;
	}

	private void MoveToVehicleSelection () {
		_currentStage = Stage.Vehicle;
		_chosenNickname = NickNameText.text.Trim();
		NextStageButton.GetComponentInChildren<Text> ().text = "join game";

		NickNameStageObj.SetActive (false);
		VehicleSelectionStageObj.SetActive (true);
	}

	void Update () {
		if (fadingOut) {
			if (FadePanelObj.color.a < 1.0f) {
				var color = FadePanelObj.color;
				color.a = Mathf.MoveTowards (FadePanelObj.color.a, 1.0f, Time.deltaTime);
				FadePanelObj.color = color;
			} else {
				fadingOut = false;
				if (_fadeOutFinish != null) {
					_fadeOutFinish ();
				}
			}
		} else {
			if (FadePanelObj.color.a > 0.0f) {
				var color = FadePanelObj.color;
				color.a = Mathf.MoveTowards (FadePanelObj.color.a, 0.0f, Time.deltaTime);
				FadePanelObj.color = color;
			}
		}
	}

	private void MoveToConnecting () {
		_currentStage = Stage.Connecting;
		NextStageButton.gameObject.SetActive (false);
		_chosenVehicle = CarPickerObj.CurrentVehicle ();
		VehicleSelectionStageObj.SetActive (false);
		ConnectingStageObj.SetActive (true);
		ConnectingTextObject.StartBlinking ();

		ClientSceneManager.Instance.OnUserRequestFindGame(_chosenNickname, _chosenVehicle);
	}

}
