using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwardImageDriver : MonoBehaviour {

	public GameObject FirstPlacePrefab;
	public GameObject SecondPlacePrefab;
	public GameObject ThirdPlacePrefab;
	public GameObject RunnerUpPrefab;

	void Start () {
		GenerateImage ();
	}

	private void GenerateImage () {

		GameObject prefab;
		NetworkCompat.NetworkLobbyPlayer thisLobbyPlayer = null;
		foreach (var lobbyPlayer in GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ()) {
			if (lobbyPlayer.isLocalPlayer) {
				thisLobbyPlayer = lobbyPlayer;
				break;
			}
		}

		if (thisLobbyPlayer == null) {
			return;
		}
			
		int place = thisLobbyPlayer.lastGameResult.FinishPosition;
		switch (place) {
		case 0:
			prefab = FirstPlacePrefab;
			break;
		case 1:
			prefab = SecondPlacePrefab;
			break;
		case 2:
			prefab = ThirdPlacePrefab;
			break;
		default:
			prefab = RunnerUpPrefab;
			break;
			
		}
		foreach (Transform child in transform) {
			GameObject.Destroy (child.gameObject);
		}

		GameObject instantiatedObj = GameObject.Instantiate (prefab);
		Transform name = instantiatedObj.transform.Find ("name");
		if (name != null) {
			Text textRenderer = name.gameObject.GetComponent<Text> ();
			textRenderer.text = thisLobbyPlayer.name;
		}

		instantiatedObj.transform.parent = transform;
		instantiatedObj.transform.localScale = Vector3.one;
		instantiatedObj.GetComponent<RectTransform> ().localPosition = Vector3.zero;
	}

}
