using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffscreenArrowRenderer : MonoBehaviour {

	public Camera CameraObject;
	public Sprite ArrowSprite;

	private Quaternion ARROW2CANVAS_ROTATION = Quaternion.Euler (new Vector3(0, 0, 0));

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
		
			// perform centering adjustments
			Vector2 centeredScreenCoords = new Vector2(
				((viewportPosition.x*canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x*0.5f)),
				((viewportPosition.y*canvasRect.sizeDelta.y)) - (canvasRect.sizeDelta.y*0.5f));

			// clamp values to screen edge
			Vector2 clampedCoords = centeredScreenCoords;

			float limitX = canvasRect.sizeDelta.x * 0.5f;
			float limitY = canvasRect.sizeDelta.y * 0.5f;

			if (clampedCoords.x > limitX) {
				clampedCoords.x = limitX;
			} else if (clampedCoords.x < -limitX) {
				clampedCoords.x = -limitX;
			}

			if (clampedCoords.y > limitY) {
				clampedCoords.y = limitY;
			} else if (clampedCoords.y < -limitY) {
				clampedCoords.y = -limitY;
			}

			float edgeDistance = Vector2.Distance (centeredScreenCoords, clampedCoords);

			if (edgeDistance > 0) {
				// calculate scale as distance from screen edge, to a limit
				float scale = Mathf.Max (MIN_SCALE, 1.0f - edgeDistance / SCALE_NORMALISER);

				Quaternion rotation = ARROW2CANVAS_ROTATION * Quaternion.AngleAxis (Mathf.Rad2Deg * Mathf.Atan2 (centeredScreenCoords.y, centeredScreenCoords.x)-90, Vector3.forward);

				sprite.GetComponent<RectTransform>().transform.localPosition = clampedCoords;

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

		RectTransform canvasRect = GetComponent<RectTransform>();

		rectTransform.parent = canvasRect.transform;

		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.rotation = ARROW2CANVAS_ROTATION;


		return obj;
	}
}
