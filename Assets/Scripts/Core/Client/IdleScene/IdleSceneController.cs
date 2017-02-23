using UnityEngine;
using System.Collections;

public class IdleSceneController : MonoBehaviour
{
	public Camera CameraObject;
	public Vector3 ObjectsRotatePoint;

	public float PanSpeed = 8.0f;		// Speed of the camera when being panned


	void Start ()
	{
		foreach (var obj in GameObject.FindObjectsOfType<IdleSceneController>()) {
			ObjectsRotatePoint = obj.transform.position;
		}
		CameraObject = Camera.main;
	}
	
	void Update ()
	{
		Vector3 axis = new Vector3 (0.1f, 1, 0);
		CameraObject.transform.RotateAround(ObjectsRotatePoint, axis, PanSpeed * Time.deltaTime);
	}
}

