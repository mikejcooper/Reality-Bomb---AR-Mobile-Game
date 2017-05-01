using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CameraDollyEventListener : MonoBehaviour {

	public GameObject LeaderboardList;
	public Animator AnimatorObj;
	public RuntimeAnimatorController AnimationController2Places;
	public RuntimeAnimatorController AnimationController3Places;

	public GameObject FirstPlaceSpawn;
	public GameObject SecondPlaceSpawn;
	public GameObject ThirdPlaceSpawn;

	public Garage Garage;

	private bool _hidingRows = true;

	void Start () {

		List<NetworkCompat.NetworkLobbyPlayer> SortedList = GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ().OrderBy (player => {
			return player.lastGameResult.FinishPosition;
		}).ToList ();

		int lobbyPlayersCount = SortedList.Count;
		if (lobbyPlayersCount > 2) {
			AnimatorObj.runtimeAnimatorController = AnimationController3Places;
		} else {
			AnimatorObj.runtimeAnimatorController = AnimationController2Places;
		}

		for (int i=0; i<3 && i<SortedList.Count; i++) {
			AddPlayerForPosition (SortedList [i], i);
		}

		if (lobbyPlayersCount == 0) {
			AnimatorObj.runtimeAnimatorController = AnimationController3Places;
			for (int i=0; i<3; i++) {
				AddPlayerForPosition (null, i);
			}
		}
	}

	private void AddPlayerForPosition(NetworkCompat.NetworkLobbyPlayer player, int pos) {
		GameObject spawnPosition;
		switch (pos) {
		case 0:
			spawnPosition = FirstPlaceSpawn;
			break;
		case 1:
			spawnPosition = SecondPlaceSpawn;
			break;
		case 2:
			spawnPosition = ThirdPlaceSpawn;
			break;
		default:
			return;
		}

		GameObject obj;
		if (player != null) {
			obj = Garage.InstantiateVehicle (Garage.CarType.MODEL, player.vehicleId, player.colour);
		} else {
			obj = Garage.InstantiateVehicle (Garage.CarType.MODEL, Garage.AvailableVehicles[0].Id, 320);
		}
		obj.transform.localRotation *= Quaternion.AngleAxis (90, Vector3.up);
		obj.transform.SetParent (spawnPosition.transform, false);
	} 

	public void OnDollyAnimationEnd() {
		_hidingRows = false;
		for (int i=0; i<LeaderboardList.transform.childCount; i++) {
			StartCoroutine (FadeIn (LeaderboardList.transform.GetChild (i).gameObject, i * 0.1f));
		}
	}

	IEnumerator FadeIn(GameObject target, float initialDelay)
	{
		yield return new WaitForSeconds(initialDelay);

		float opacity = 0.0f;

		while(opacity < 1.0f) 
		{ 
			opacity += 0.1f;
			SetRowOpacity (target, opacity);
			yield return new WaitForSeconds(0.05f);
		}
	}

	void Update () {
		if (_hidingRows) {
			foreach (Transform row in LeaderboardList.transform) {
				SetRowOpacity (row.gameObject, 0);
			}
		}
	}

	private void SetRowOpacity (GameObject row, float opacity) {
		foreach (var text in row.GetComponentsInChildren<Text>()) {
			var color = text.color;
			color.a = opacity;
			text.color = color;
		}

		foreach (var image in row.GetComponentsInChildren<RawImage>()) {
			var color = image.color;
			color.a = opacity * 0.28f;
			image.color = color;
		}
	}


}
