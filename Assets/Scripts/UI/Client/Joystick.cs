using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler{

	public Canvas Canvas;
	public RectTransform JoystickRectTransform;
	public float JoysitckPositionFromBR;
	public float JoysitckSize;

	public GameObject Front;
	public List<GameObject> DecoObjects;
	public int MinRotationSpeed;
	public int MaxRotationSpeed;
	public bool Active = false;
	public Vector2 NormalisedVector;


	private List<int> _decoSpeeds;
	private System.Random _random;
	private float _decoAlpha = 0.0f;
	private Vector3 _currentPos;


	void Start () {
		if(Canvas != null && JoystickRectTransform != null)
			PositionJoystickForScreen ();

		_random = new System.Random ();
		_decoSpeeds = new List<int> ();
		foreach (var deco in DecoObjects) {
			var speed = _random.Next (MinRotationSpeed, MaxRotationSpeed);
			speed *= (_random.Next (1, 2) == 1 ? -1 : 1);
			_decoSpeeds.Add(speed);
			var color = deco.GetComponent<Image> ().color;
			color.a = 0;
			deco.GetComponent<Image> ().color = color;

		}
	}

	void FixedUpdate () {
		if (Active) {
			_decoAlpha = Mathf.Min(1.0f, _decoAlpha + 0.1f);
		} else {
			_decoAlpha = Mathf.Max(0.0f, _decoAlpha - 0.1f);
		}

		for (int i = 0; i < DecoObjects.Count; i++) {
			var deco = DecoObjects [i];
			var color = deco.GetComponent<Image> ().color;
			color.a = _decoAlpha;
			deco.GetComponent<Image> ().color = color;
			var rotation = _decoSpeeds [i];
			deco.transform.Rotate (Vector3.forward * rotation);
			if (_random.Next (100) == 5) {
				_decoSpeeds [i] = _random.Next (MinRotationSpeed, MaxRotationSpeed);
			}

			if (_random.Next (100) == 5) {
				_decoSpeeds [i] *= -1;
			}
		}

		if (!Active) {
			SetPosition(Vector3.Slerp (_currentPos, Vector3.zero, 1.0f - _decoAlpha));
		}
	}

	private void OnPointerEvent(PointerEventData ped) {

		Vector2 pos; 

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle (GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out pos)) {
		}

		var offset = pos;//new Vector2 (transform.position.x, transform.position.y) - ped.position;

		var maxRadius = GetComponent<RectTransform> ().sizeDelta.x / 2.0f;

		var offsetLength = offset.magnitude;

		var length = Mathf.Min (maxRadius, offsetLength);

		NormalisedVector = new Vector2 (offset.x, offset.y).normalized * (length / maxRadius);

		SetPosition (NormalisedVector * length);
	}

	private void PositionJoystickForScreen(){

		//----------------------------Positioning -----------------------------------------------

		Vector2 lXY = Canvas.GetComponent<RectTransform> ().sizeDelta;

		// Pixels per inch for current device
		float DPI = Screen.dpi;

		// Screen x and y pixel size 
		int pY = Screen.height;
		int pX = Screen.width;

		// Local x and y screen size
		float lX = lXY [0];
		float lY = lXY [1];

		// local size per pixel 
		float dpXlX = lX/pX;
		float dpYlY = lY/pY;

		// Position from bottom right in inches 
		float xInchShift = JoysitckPositionFromBR;
		float yInchShift = JoysitckPositionFromBR;

		// Set joystick position to bottom right + Shift from bottom right 
		Vector3 newPosition = new Vector3(lX/2,-lY/2,0) + new Vector3(-xInchShift * DPI * dpXlX, yInchShift * DPI * dpYlY, 0);

		//----------------------------SCALING -----------------------------------------------

		// Size in inches 
		float wInchSize = JoysitckSize;
		float hInchSize = JoysitckSize;

		// local obj size width and hieght 
		float lW = JoystickRectTransform.rect.width;
		float lH = JoystickRectTransform.rect.height;

		// New local scale
		float scaleW = DPI * wInchSize * dpXlX / lW;
		float scaleH = DPI * hInchSize * dpYlY / lH;
		Vector3 newScale = new Vector3(scaleW, scaleH, 1);

		//----------------------------Set Position and Scale -----------------------------------------------

		// Set new position and scale
		JoystickRectTransform.localScale = newScale;

		JoystickRectTransform.localPosition = newPosition;

	}

	public virtual void OnDrag(PointerEventData ped) {
		OnPointerEvent (ped);
	}

	public void OnPointerDown (PointerEventData eventData) {
		Active = true;
		OnPointerEvent (eventData);
	}

	public void OnPointerUp (PointerEventData eventData) {
		Active = false;
		NormalisedVector = Vector3.zero;
	}

	private void SetPosition (Vector2 pos) {
		_currentPos = pos;
		Front.GetComponent<RectTransform> ().localPosition = pos;
	}
}
