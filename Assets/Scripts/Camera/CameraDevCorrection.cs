using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDevCorrection : MonoBehaviour {

	public float yPosition;
	public float xRotation;
	public float yRotation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position = new Vector3 (gameObject.transform.position.x, yPosition, gameObject.transform.position.z);
		gameObject.transform.rotation = Quaternion.Euler (xRotation, yRotation, gameObject.transform.rotation.z);
	}
}
