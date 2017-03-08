using UnityEngine;
using System.Collections;

public class ServerIdleSceneController : MonoBehaviour
{
	public GameObject Car;
	public Transform RoatateAround;

	private Camera _cameraObject;
	private Vector3 _cameraStartPosition;

	private float _speed = 0.2f;
	private float _width = 3;
	private float _height = 0.1f;
	private float _z = 1;
	private float _timeCounter = 0;



	void Start ()
	{
		// Camera
		_cameraObject = Camera.main;
		_cameraStartPosition = _cameraObject.transform.position;
	}

	void Update () {
		// Camera Movement
		_timeCounter += Time.deltaTime * _speed;
		float x = Mathf.Sin (_timeCounter) * _width;
		float y = Mathf.Cos (_timeCounter) * _height;
		float z = Mathf.Sin (0.5f*_timeCounter) * _z;
		_cameraObject.transform.position = new Vector3 (x, y, z) + _cameraStartPosition;
		_cameraObject.transform.LookAt(RoatateAround);

		// Car Movement
		Car.transform.Rotate(Vector3.up * _timeCounter, 0.2f);
	}
}

