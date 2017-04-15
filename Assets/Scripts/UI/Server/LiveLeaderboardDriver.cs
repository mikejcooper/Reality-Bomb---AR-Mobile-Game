using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LiveLeaderboardDriver : MonoBehaviour {

	public GameObject RowPrefab;
	public Sprite CarSilhouetteSprite;
	public Sprite BombSilhouetteSprite;
	
	void Start () {
		
	}

	void Update () {
		CarController[] cars = FindObjectsOfType<CarController> ();

		List<CarController> orderedcars = cars.OrderBy (o => -o.Lifetime).ToList ();

		foreach (var carController in cars) {
			PlayerDataManager.PlayerData playerData = ServerSceneManager.Instance.GetPlayerDataById (carController.ServerId);

			var entryTransform = transform.Find (playerData.Name);
			GameObject entry;
			if (entryTransform != null) {
				entry = entryTransform.gameObject;
			} else {
				entry = GameObject.Instantiate (RowPrefab);
				entry.name = playerData.Name;
				entry.transform.SetParent (transform, false);
			}

			entry.GetComponent<RectTransform> ().SetSiblingIndex (orderedcars.IndexOf (carController));

			entry.transform.Find ("Pos").GetComponent<Text> ().text = (orderedcars.IndexOf (carController) + 1).ToString ();
			if (carController.Alive && carController.HasBomb) {
				entry.transform.Find ("Icon").GetComponent<Image> ().sprite = BombSilhouetteSprite;
				entry.transform.Find ("Icon").GetComponent<Image> ().color = Color.white;
			} else {
				entry.transform.Find ("Icon").GetComponent<Image> ().sprite = CarSilhouetteSprite;
				entry.transform.Find ("Icon").GetComponent<Image> ().color = Color.HSVToRGB (playerData.colour / 360f, 1f, 0.8f);
			}

			entry.transform.Find ("Tag").GetComponent<Text> ().text = playerData.Name;

			if (carController.Lifetime > 0f) {
				entry.transform.Find ("TimeLeft").GetComponent<Text> ().text = string.Format ("{0}s", carController.Lifetime.ToString ("n2"));
			} else {
				entry.transform.Find ("TimeLeft").GetComponent<Text> ().text = "out";
			}
		}

		
	}

}


