using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Carousel : MonoBehaviour {

	public float XRadius = 1;
	public float ZRadius = 1;

	public float DefaultRotationDegrees = 0;
	public float FocusRotationRate = 1;
	public int FocusIndex = 0;

	private const float START_OFFSET = - Mathf.PI / 2.0f;
	private float _currentLookAngle = START_OFFSET;
	private float _targetLookAngle;

	void OnValidate () {
		_targetLookAngle = _currentLookAngle;
		ArrangeChildren ();
	}

	void Update () {
		ArrangeChildren ();

		var childCount = transform.childCount;

		_targetLookAngle = (FocusIndex * 2 * Mathf.PI / (float)childCount) + START_OFFSET;

		_currentLookAngle = Mathf.MoveTowards (_currentLookAngle, _targetLookAngle, 5 * Time.deltaTime);
	}

	private void ArrangeChildren () {
		var childCount = transform.childCount;
		Quaternion defaultRotation = Quaternion.AngleAxis (DefaultRotationDegrees, Vector3.up);

		for (int i=0; i<childCount; i++) {
			Transform child = transform.GetChild (i);
			float angle = -(i * 2 * Mathf.PI / (float)childCount) + _currentLookAngle;
			child.localPosition = new Vector3 (XRadius * Mathf.Cos (angle), 0, ZRadius * Mathf.Sin (angle));



			if (Application.isPlaying && i == ((FocusIndex+100*childCount) % childCount)) {
				child.localRotation *= Quaternion.AngleAxis (FocusRotationRate * Time.deltaTime, Vector3.up);
			} else {
				child.localRotation = Quaternion.RotateTowards (child.localRotation, defaultRotation, 100 * Time.deltaTime);
			}

		}
	}

}
