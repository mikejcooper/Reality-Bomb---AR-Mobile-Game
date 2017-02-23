using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparingGame : MonoBehaviour {

	public Sprite WaitingForPlayersToConnect;
	public Sprite Three;
	public Sprite Two;
	public Sprite One;
	public Sprite GO;

	GameObject WaitingForPlayersToConnectObj;
	GameObject ThreeObj;
	GameObject TwoObj;
	GameObject OneObj;
	GameObject GoObj;
//
//
//

	private Dictionary<CarProperties, GameObject> _sprites;

	void Start () {
		WaitingForPlayersToConnectObj = CreateSprite (WaitingForPlayersToConnect);
		ThreeObj = CreateSprite (Three);
		TwoObj = CreateSprite (Two);
		OneObj = CreateSprite (One);
		GoObj = CreateSprite (GO);
		StartCoroutine(StartCountDown ());

	}
		
	IEnumerator StartCountDown() {
		WaitingForPlayersToConnectObj.SetActive (true);
		yield return new WaitForSeconds (5f);
		WaitingForPlayersToConnectObj.SetActive (false);

		yield return new WaitForSeconds (1f);
		ThreeObj.SetActive (true);
		yield return new WaitForSeconds (3f);
		ThreeObj.SetActive (false);
		yield return new WaitForSeconds (1f);
		TwoObj.SetActive (true);
		yield return new WaitForSeconds (3f);
		TwoObj.SetActive (false);
		yield return new WaitForSeconds (1f);
		OneObj.SetActive (true);
		yield return new WaitForSeconds (3f);
		OneObj.SetActive (false);
		yield return new WaitForSeconds (1f);		
		GoObj.SetActive (true);
		yield return new WaitForSeconds (3f);
		GoObj.SetActive (false);
	}

	void Update () {
//		foreach (var car in GameObject.FindObjectsOfType<CarProperties>()) {
//			GameObject sprite;
//			if (!_sprites.ContainsKey (car)) {
//				sprite = CreateSprite ();
//				_sprites[car] = sprite;
//			} else {
//				sprite = _sprites [car];
//			}
//
//			RectTransform canvasRect = GetComponent<RectTransform>();
//
//			// project onto viewport
//			Vector2 viewportPosition = CameraObject.WorldToViewportPoint(car.transform.position);
//
//			// perform centering adjustments
//			Vector2 centeredScreenCoords = new Vector2(
//				((viewportPosition.x*canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x*0.5f)),
//				((viewportPosition.y*canvasRect.sizeDelta.y)) - (canvasRect.sizeDelta.y*0.5f));
//
//			// clamp values to screen edge
//			Vector2 clampedCoords = centeredScreenCoords;
//
//			float limitX = canvasRect.sizeDelta.x * 0.5f;
//			float limitY = canvasRect.sizeDelta.y * 0.5f;
//
//			if (clampedCoords.x > limitX) {
//				clampedCoords.x = limitX;
//			} else if (clampedCoords.x < -limitX) {
//				clampedCoords.x = -limitX;
//			}
//
//			if (clampedCoords.y > limitY) {
//				clampedCoords.y = limitY;
//			} else if (clampedCoords.y < -limitY) {
//				clampedCoords.y = -limitY;
//			}
//
//			float edgeDistance = Vector2.Distance (centeredScreenCoords, clampedCoords);
//
//			if (edgeDistance > 0) {
//				// calculate scale as distance from screen edge, to a limit
//				float scale = Mathf.Max (MIN_SCALE, 1.0f - edgeDistance / SCALE_NORMALISER);
//
//				Quaternion rotation = ARROW2CANVAS_ROTATION * Quaternion.AngleAxis (Mathf.Rad2Deg * Mathf.Atan2 (centeredScreenCoords.y, centeredScreenCoords.x)-90, Vector3.forward);
//
//				sprite.GetComponent<RectTransform>().transform.localPosition = clampedCoords;
//
//				sprite.GetComponent<RectTransform>().transform.rotation = rotation;
//
//				sprite.GetComponent<RectTransform> ().transform.localScale = scale * Vector3.one;
//
//				sprite.SetActive (true);
//			} else {
//				// a value of 0 means the player is on-screen
//				sprite.SetActive (false);
//			}
//
//		}
	}

	private GameObject CreateSprite (Sprite sprite) {
		GameObject obj = new GameObject();

		Image image = obj.AddComponent<Image> ();
		RectTransform rectTransform = obj.GetComponent<RectTransform> ();

		image.sprite = sprite;


		RectTransform canvasRect = GetComponent<RectTransform>();

		rectTransform.parent = canvasRect.transform;

		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one;

		obj.SetActive (false);
		return obj;
	}
}
