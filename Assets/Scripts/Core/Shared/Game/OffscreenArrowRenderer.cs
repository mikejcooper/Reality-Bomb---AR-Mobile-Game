using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffscreenArrowRenderer : MonoBehaviour {

	public Camera CameraObject;
	public Sprite ArrowSprite;

	// The distance over which we change arrow scale.
	// If distance is greater than this value, it is
	// clamped.
	private const float SCALE_NORMALISER = 100f;
	private const float MIN_SCALE = 0.4f;

	private Dictionary<CarProperties, GameObject> _sprites;

	void Start () {
		_sprites = new Dictionary<CarProperties, GameObject>();
	}

	void Update () {
		foreach (var car in GameObject.FindObjectsOfType<CarProperties>()) {
			GameObject sprite;
			if (!_sprites.ContainsKey (car)) {
				sprite = CreateSprite ();
				_sprites[car] = sprite;
			} else {
				sprite = _sprites [car];
			}

			RectTransform canvasRect = GetComponent<RectTransform>();

			// project onto viewport
			Vector2 viewportPosition = CameraObject.WorldToViewportPoint(car.transform.position);

			// perform adjustments
			Vector2 screenCoords = new Vector2(
				((viewportPosition.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
				((viewportPosition.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)));

			// shift coordinates to center
			Vector2 centeredScreenCoords = new Vector2 (
               ((canvasRect.rect.width / 2.0f) + screenCoords.x),
               ((canvasRect.rect.height / 2.0f) + screenCoords.y));



			// clamp values to screen edge
			Vector2 clampedCoords = centeredScreenCoords;

			if (clampedCoords.x > canvasRect.rect.width) {
				clampedCoords.x = canvasRect.rect.width;
			} else if (clampedCoords.x < 0) {
				clampedCoords.x = 0;
			}

			if (clampedCoords.y > canvasRect.rect.height) {
				clampedCoords.y = canvasRect.rect.height;
			} else if (clampedCoords.y < 0) {
				clampedCoords.y = 0;
			}


			Quaternion rotation = Quaternion.AngleAxis (Mathf.Rad2Deg * Mathf.Atan2 (screenCoords.y, screenCoords.x)-90, transform.forward);


			float edgeDistance = Vector2.Distance (centeredScreenCoords, clampedCoords);

			if (edgeDistance > 0) {
				// calculate scale as distance from screen edge, to a limit
				float scale = Mathf.Max (MIN_SCALE, 1.0f - edgeDistance / SCALE_NORMALISER);

				sprite.GetComponent<RectTransform>().transform.position = clampedCoords;

				sprite.GetComponent<RectTransform>().transform.rotation = rotation;

				sprite.GetComponent<RectTransform> ().transform.localScale = scale * Vector3.one;

				sprite.SetActive (true);
			} else {
				// a value of 0 means the player is on-screen
				sprite.SetActive (false);
			}
		
		}
	}

	private GameObject CreateSprite () {
		GameObject obj = new GameObject();

		Image image = obj.AddComponent<Image> ();
		RectTransform rectTransform = obj.GetComponent<RectTransform> ();

		image.sprite = ArrowSprite;

		// we update the RectTransform's pivot because it doesn't seem to inherit from the sprite
		rectTransform.pivot = new Vector2(ArrowSprite.pivot.x / ArrowSprite.rect.width, ArrowSprite.pivot.y / ArrowSprite.rect.height) ;

		obj.transform.parent = transform;

		return obj;
	}
}
