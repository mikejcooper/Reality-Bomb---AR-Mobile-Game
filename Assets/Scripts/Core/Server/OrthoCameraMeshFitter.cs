using System;
using UnityEngine;

public class OrthoCameraMeshFitter : MonoBehaviour {

	public GameObject Mesh;

	void Start () {

		if (Mesh != null) {
			Bounds bounds = Mesh.GetComponent<MeshRenderer> ().bounds;
			PositionFromBounds (bounds);
		}

	}

	public void PositionFromBounds (Bounds bounds) {

		float boundsRatio = Math.Max (bounds.extents.x / (float) bounds.extents.z, 1.0f);

		Camera cam = GetComponent<Camera> ();

		float combined = boundsRatio / cam.aspect;

		transform.position = new Vector3 (bounds.center.x, bounds.max.y + cam.nearClipPlane + 1, bounds.center.z);

		cam.orthographicSize = bounds.extents.z * combined;
	}
}

