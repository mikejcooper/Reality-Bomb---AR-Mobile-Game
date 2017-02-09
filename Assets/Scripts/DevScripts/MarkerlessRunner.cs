using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerlessRunner : MonoBehaviour {

	public GameObject camera;
	public GameObject markerScene;

	public float cameraXPosition;
	public float cameraYPosition;
	public float cameraZPosition;
	public float cameraXRotation;
	public float cameraYRotation;
	public float cameraZRotation;

	// Use this for initialization
	void Start () {
		if (Application.isEditor || !Application.isMobilePlatform) {
			markerScene.GetComponent<ARTrackedObject> ().enabled = false;
			foreach (Transform child in markerScene.transform) {
				child.gameObject.SetActive (true);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.isEditor || !Application.isMobilePlatform) {
			camera.transform.position = new Vector3 (cameraXPosition, cameraYPosition, cameraZPosition);
			camera.transform.rotation = Quaternion.Euler (cameraXRotation, cameraYRotation, cameraZRotation);



		}
	}
}
