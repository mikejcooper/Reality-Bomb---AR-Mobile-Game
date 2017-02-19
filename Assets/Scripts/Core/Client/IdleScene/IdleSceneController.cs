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
		CameraObject.transform.RotateAround(ObjectsRotatePoint, Vector3.up, PanSpeed * Time.deltaTime);
	}
}

