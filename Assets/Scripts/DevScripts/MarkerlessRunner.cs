using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerlessRunner : MonoBehaviour {

	public GameObject camera;
	public GameObject markerScene;

	public float cameraYPosition;
	public float cameraXRotation;
	public float cameraYRotation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.isEditor) {
			camera.transform.position = new Vector3 (camera.transform.position.x, cameraYPosition, camera.transform.position.z);
			camera.transform.rotation = Quaternion.Euler (cameraXRotation, cameraYRotation, camera.transform.rotation.z);

			markerScene.GetComponent<ARTrackedObject> ().enabled = false;
			foreach (Transform child in markerScene.transform) {
				child.gameObject.SetActive (true);
			}

		}
	}
}
