using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawning
{

	public static Vector3 FindSpawnLocation (GameObject worldMesh) {

		Bounds bounds = worldMesh.transform.GetComponent<MeshRenderer> ().bounds;
		Vector3 center = bounds.center;

		for (int i = 0; i < 30; i++) {
			float x = UnityEngine.Random.Range (center.x - (bounds.size.x / 2), center.x + (bounds.size.x / 2));
			float z = UnityEngine.Random.Range (center.x - (bounds.size.z / 2), center.z + (bounds.size.z / 2));

			Vector3 position = new Vector3 (x, center.y + bounds.size.y, z);
			RaycastHit hit;

			if (Physics.Raycast (position, Vector3.down, out hit, bounds.size.y * 2)) {
				position.y = hit.point.y;

				return position;
			}
		}
		return Vector3.zero;
	}

	public static void Reposition(GameObject worldMesh, bool hasAuthority, Rigidbody _rigidbody, GameObject gameObject)
	{
		DebugConsole.Log ("repositioning");
		if (hasAuthority) {
			DebugConsole.Log ("repositioning with authority");

			Debug.Log ("Repositioning car");

			//Set velocities to zero
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;

			Vector3 position = FindSpawnLocation (worldMesh);

			if (position != Vector3.zero) {
				DebugConsole.Log ("unfreezing");
				// now unfreeze and show

				gameObject.SetActive (true);
				_rigidbody.isKinematic = false;

				_rigidbody.position = position;
			}

		}
	}
}


