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

			var carName = carController.LobbyPlayer().nickname;
			var entryTransform = transform.Find (carName);
			GameObject entry;
			if (entryTransform != null) {
				entry = entryTransform.gameObject;
			} else {
				entry = GameObject.Instantiate (RowPrefab);
				entry.name = carName;
				entry.transform.SetParent (transform, false);
			}

			entry.GetComponent<RectTransform> ().SetSiblingIndex (orderedcars.IndexOf (carController));

			entry.transform.Find ("Pos").GetComponent<Text> ().text = (orderedcars.IndexOf (carController) + 1).ToString ();
			if (carController.Alive && carController.HasBomb) {
				entry.transform.Find ("Icon").GetComponent<Image> ().sprite = BombSilhouetteSprite;
				entry.transform.Find ("Icon").GetComponent<Image> ().color = Color.white;
			} else {
				entry.transform.Find ("Icon").GetComponent<Image> ().sprite = CarSilhouetteSprite;
				var colour = carController.GetComponent<CarProperties> ().OriginalHue;
				entry.transform.Find ("Icon").GetComponent<Image> ().color = Color.HSVToRGB (colour / 360f, 1f, 0.8f);
			}

			entry.transform.Find ("Tag").GetComponent<Text> ().text = carName;

			if (carController.Lifetime > 0f) {
				entry.transform.Find ("TimeLeft").GetComponent<Text> ().text = string.Format ("{0}s", carController.Lifetime.ToString ("n2"));
			} else {
				entry.transform.Find ("TimeLeft").GetComponent<Text> ().text = "out";
			}
		}

		
	}

}


