using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
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

	public static int ChooseRandomPlayerConnectionId () {
		List<int> activeConnectionIds = new List<int>();
		for (int i=0; i<UnityEngine.Networking.NetworkServer.connections.Count; i++) {
			if (UnityEngine.Networking.NetworkServer.connections [i] != null &&
				UnityEngine.Networking.NetworkServer.connections [i].isConnected) {
				activeConnectionIds.Add (UnityEngine.Networking.NetworkServer.connections [i].connectionId);
			}
		}
		int bombPlayerIndex = UnityEngine.Random.Range(0, activeConnectionIds.Count);
		DebugConsole.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, activeConnectionIds.Count));

		return activeConnectionIds [bombPlayerIndex];
	}
}


