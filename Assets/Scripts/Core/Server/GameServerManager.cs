using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class GameServerManager : MonoBehaviour {


	// todo: get rid of this once we transition to keyboard shortcuts
	public GameObject ButtonPrefab;
	public GameObject GameStartingDialogObj;
	public Powerups.GamePowerUpManager PowerupManager;
	public GameObject Canvas;
	public GameObject ServerUIPrefab;
	public GameObject ARToolkitObj;
	public Material MeshMaterial;

	private GameObject _serverUI;

	void Start () {
		if (!NetworkServer.active) {
			Debug.LogWarning ("not server, exiting");
			Destroy (this);
			return;
		}

		PowerupManager.enabled = false;

		SetupUI ();
		if (ServerSceneManager.Instance != null) {
			if (ServerSceneManager.Instance.AreAllGamePlayersLoaded ()) {
				AllPlayersLoadedListener ();
			} else {
				ServerSceneManager.Instance.OnAllPlayersLoadedEvent += AllPlayersLoadedListener;
			}
		}

		StartCoroutine (PollForWorldMesh()); 
	}

	void OnDestroy () {
		if (ServerSceneManager.Instance != null) {
			ServerSceneManager.Instance.OnAllPlayersLoadedEvent -= AllPlayersLoadedListener;
		}
	}

	IEnumerator PollForWorldMesh()
	{
		while(FindObjectOfType<GameManager>().WorldMesh == null) 
		{ 
			yield return new WaitForSeconds(0.1f);
		}

		SetupMeshDependentUI ();
	}

	void OnGUI() {

		if (Event.current.Equals (Event.KeyboardEvent ("m"))) {
			if(AudioListener.volume == 0.0f) {
				AudioListener.volume = 1.0f;
			}
			else {
				AudioListener.volume = 0.0f;
			}
		}

	}

	private void SetupMeshDependentUI () {
		OrthoCameraMeshFitter camFitter = _serverUI.GetComponentInChildren<OrthoCameraMeshFitter> ();

		if (FindObjectOfType<GameManager>().WorldMesh != null) {
			MeshRenderer renderer = FindObjectOfType<GameManager>().WorldMesh.ground.GetComponent<MeshRenderer> ();
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

	}

	void AllPlayersLoadedListener () {
		// this can be called multiple times, so remove listener
		ServerSceneManager.Instance.OnAllPlayersLoadedEvent -= AllPlayersLoadedListener;
		PowerupManager.enabled = true;
		Invoke ("StartCountdown", 5);
	}

	private void StartCountdown () {
		FindObjectOfType<GameManager>().StartCountdown();
	}


}
	

