using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler{

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
		var offset = new Vector2 (transform.position.x, transform.position.y) - ped.position;

		var radius = GetComponent<RectTransform> ().sizeDelta.x / 2.0f;

		var length = Mathf.Min (radius, offset.magnitude);

		NormalisedVector = new Vector2 (-offset.x, -offset.y).normalized;

		SetPosition (NormalisedVector * length);
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
	}

	private void SetPosition (Vector2 pos) {
		_currentPos = pos;
		Front.GetComponent<RectTransform> ().localPosition = pos;
		foreach (var deco in DecoObjects) {
			deco.GetComponent<RectTransform> ().localPosition = pos;
		}
	}
}
