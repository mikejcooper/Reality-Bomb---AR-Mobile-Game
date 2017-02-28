using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

	public Camera cameraObj;

	void Update () {
		transform.rotation = Quaternion.LookRotation(-cameraObj.transform.forward);
	}
}
