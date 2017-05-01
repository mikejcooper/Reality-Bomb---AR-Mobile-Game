using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraJitter : MonoBehaviour {

	public float SmoothSpeed = 0.1f;
	public float JitterAmount = 0.1f;
	public int Frequency = 10;

	private Vector3 velocity = Vector3.zero;

	private Vector3 _targetPos;
	private int _counter = 0;

	void Update () {
		if (_counter % Frequency == 0) {
			_targetPos = new Vector3 (Random.Range (-JitterAmount, JitterAmount), Random.Range (-JitterAmount, JitterAmount), Random.Range (-JitterAmount, JitterAmount));
		}
		_counter++;
		transform.localPosition = Vector3.SmoothDamp (transform.localPosition, _targetPos, ref velocity, SmoothSpeed);
	}
}
