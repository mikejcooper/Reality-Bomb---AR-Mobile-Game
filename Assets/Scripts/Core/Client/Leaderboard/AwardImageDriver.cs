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
		PlayerDataManager.PlayerData playerData;

		if (ClientSceneManager.Instance == null || ClientSceneManager.Instance.GetThisPlayerData () == null) {
			Debug.LogWarning ("making up our own player data because this client's player data is null");
			playerData = new PlayerDataManager.PlayerData ();
			playerData.Name = NameGenerator.GenerateName ();
			playerData.FinishPosition = new System.Random ().Next (4);
		} else {
			playerData = ClientSceneManager.Instance.GetThisPlayerData ();
		}
			
		int place = playerData.FinishPosition;
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
			textRenderer.text = playerData.Name;
		}

		instantiatedObj.transform.parent = transform;
		instantiatedObj.transform.localScale = Vector3.one;
		instantiatedObj.GetComponent<RectTransform> ().localPosition = Vector3.zero;
	}

}
