using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class GameServerManager : MonoBehaviour {


	// todo: get rid of this once we transition to keyboard shortcuts
	public GameObject ButtonPrefab;
	public GameObject GameStartingDialogObj;
	public GameObject Canvas;
	public GameManager GameManagerObj;
	public GameObject ServerUIPrefab;
	public GameObject ARToolkitObj;
	public Material MeshMaterial;

	private Button _serverStartButton;
	private GameObject _button;
	private GameObject _serverUI;

	void Start () {
		if (!NetworkServer.active) {
			Debug.LogWarning ("not server, exiting");
			Destroy (this);
			return;
		}

		Debug.Log ("GameServerManager Start");

		SetupUI ();
		if (ServerSceneManager.Instance != null) {
			ServerSceneManager.Instance.OnAllPlayersLoadedEvent += OnAllPlayersLoaded;
		}

		StartCoroutine (PollForWorldMesh()); 
	}

	void OnDestroy () {
		ServerSceneManager.Instance.OnAllPlayersLoadedEvent -= OnAllPlayersLoaded;
	}

	void OnAllPlayersLoaded () {
		Debug.Log ("GameServerManager OnAllPlayersLoaded");
		if (_serverStartButton != null) {
			_serverStartButton.enabled = true;	
		}
	}

	IEnumerator PollForWorldMesh()
	{
		while(GameManagerObj.WorldMesh == null) 
		{ 
			yield return new WaitForSeconds(0.1f);
		}

		SetupMeshDependentUI ();
	}

	private void SetupMeshDependentUI () {
		OrthoCameraMeshFitter camFitter = _serverUI.GetComponentInChildren<OrthoCameraMeshFitter> ();

		if (GameManagerObj.WorldMesh != null) {
			MeshRenderer renderer = GameManagerObj.WorldMesh.GetComponent<MeshRenderer> ();
			Bounds bounds = renderer.bounds;
			Material[] mats = renderer.materials;
			mats[0] = MeshMaterial;
			renderer.materials = mats;
			camFitter.PositionFromBounds (bounds);
		} else {
			Debug.LogError ("Couldn't find world mesh");
		}
	}
		
	private void SetupUI () {
		Debug.LogWarning ("setting up server UI");

		var joystick = GameObject.FindObjectOfType<Joystick> ();
		if (joystick != null) {
			joystick.gameObject.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find joystick. Check this!");
		}

		var healthbar = GameObject.Find ("HealthBar");
		if (healthbar != null) {
			healthbar.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find health bar. Check this!");
		}

		var markerAlert = GameObject.Find ("MarkerAlert");
		if (markerAlert != null) {
			markerAlert.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find marker alert. Check this!");
		}

		var spectatingText = GameObject.Find ("SpectatingText");
		if (spectatingText != null) {
			spectatingText.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find spectating text. Check this!");
		}

		var offscreenArrows = GameObject.Find ("Offscreen Arrows");
		if (offscreenArrows != null) {
			offscreenArrows.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find offscreen arrows. Check this!");
		}

		ARToolkitObj.SetActive (false);

		var artoolkitCamera = GameObject.Find ("Video background");
		if (artoolkitCamera != null) {
			artoolkitCamera.SetActive (false);
		} else {
			Debug.LogWarning ("Could not find artoolkit camera. Check this!");
		}

		Destroy (GameStartingDialogObj);
		GameStartingDialogObj = null;

		_serverUI = GameObject.Instantiate (ServerUIPrefab);

		// this is pretty hacky dev stuff
		_button = GameObject.Instantiate (ButtonPrefab);

		_button.transform.parent = Canvas.transform;

		_button.GetComponentInChildren<UnityEngine.UI.Text> ().text = "Start";
		_serverStartButton = _button.GetComponent<UnityEngine.UI.Button> ();
		_serverStartButton.enabled = false;
		_serverStartButton.onClick.AddListener (ButtonClickListener);
		_button.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;



	}

	void ButtonClickListener () {
		if (_serverStartButton != null) {
			_serverStartButton.onClick.RemoveListener (ButtonClickListener);
		}
		Debug.Log("click");
		Destroy(_button);
		GameManagerObj.StartCountdown();
	}

}
	

