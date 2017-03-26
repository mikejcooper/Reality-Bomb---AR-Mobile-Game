using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerlessRunner : MonoBehaviour {

	public GameObject CameraObject;
	public GameObject MarkerScene;

	public float CameraXPosition;
	public float CameraYPosition;
	public float CameraZPosition;
	public float CameraXRotation;
	public float CameraYRotation;
	public float CameraZRotation;

	// Use this for initialization
	void Start () {
		if (Application.isEditor || !Application.isMobilePlatform) {
			MarkerScene.GetComponent<ARTrackedObject> ().enabled = false;
			foreach (Transform child in MarkerScene.transform) {
				child.gameObject.SetActive (true);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.isEditor || !Application.isMobilePlatform) {
			CameraObject.transform.position = new Vector3 (CameraXPosition, CameraYPosition, CameraZPosition);
			CameraObject.transform.rotation = Quaternion.Euler (CameraXRotation, CameraYRotation, CameraZRotation);
		}
	}
}
