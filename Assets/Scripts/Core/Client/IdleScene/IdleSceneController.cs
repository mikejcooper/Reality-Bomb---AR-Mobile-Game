using UnityEngine;
using System.Collections;

public class IdleSceneController : MonoBehaviour
{
	private Camera CameraObject;

	private Transform _lookAt;
	private Vector3 _startPosition;

	private float _speed = 0.2f;
	private float _width = 3;
	private float _height = 0.2f;
	private float _z = 1;



	void Start ()
	{
		CameraObject = Camera.main;
		_startPosition = CameraObject.transform.position;
		_lookAt = FindObjectOfType<IdleSceneController> ().transform;
	}
//	
//	void Update ()
//	{
//		Vector3 axis = new Vector3 (0, 1, 0);
//		CameraObject.transform.RotateAround(ObjectsRotatePoint, axis, PanSpeed * Time.deltaTime);
//	}
//

	float timeCounter = 0;

	void Update () {
		timeCounter += Time.deltaTime * _speed;
		float x = Mathf.Cos (timeCounter) * _width;
		float y = Mathf.Sin (timeCounter) * _height;
		float z = Mathf.Sin (timeCounter) * _z;
		CameraObject.transform.position = new Vector3 (x, y, z) + _startPosition;
		CameraObject.transform.LookAt(_lookAt);
	}
}

