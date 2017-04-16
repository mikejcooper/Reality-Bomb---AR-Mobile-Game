using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameUtils
{
	public static int ChooseRandomPlayerConnectionId () {
		List<int> activeConnectionIds = new List<int>();
		for (int i=0; i<UnityEngine.Networking.NetworkServer.connections.Count; i++) {
			if (UnityEngine.Networking.NetworkServer.connections [i] != null &&
				UnityEngine.Networking.NetworkServer.connections [i].isConnected) {
				activeConnectionIds.Add (UnityEngine.Networking.NetworkServer.connections [i].connectionId);
			}
		}
		int bombPlayerIndex = UnityEngine.Random.Range(0, activeConnectionIds.Count);
		Debug.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, activeConnectionIds.Count));

		return activeConnectionIds [bombPlayerIndex];
	}
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
				return position + new Vector3(0.0f,1.0f,0.0f);
			}
		}
		return Vector3.zero;
	}
		
	public static Vector3 FindSpawnLocationInsideConvexHull(GameMapObjects gameMapObjects){
		Vector3[] convexHull = new Vector3[gameMapObjects.boundary.GetComponent<MeshFilter> ().mesh.vertices.Length / 2];
		Array.Copy (gameMapObjects.boundary.GetComponent<MeshFilter> ().mesh.vertices, 0, convexHull, 0, gameMapObjects.boundary.GetComponent<MeshFilter> ().mesh.vertices.Length / 2);
		convexHull = MinimizeConvexHull (convexHull);

		Bounds bounds = gameMapObjects.boundary.transform.GetComponent<MeshRenderer> ().bounds;
		Vector3 center = bounds.center;

		for (int i = 0; i < 30; i++) {
			float x = UnityEngine.Random.Range (center.x - (bounds.size.x / 2), center.x + (bounds.size.x / 2));
			float z = UnityEngine.Random.Range (center.z - (bounds.size.z / 2), center.z + (bounds.size.z / 2));

			Vector3 position = new Vector3 (x, center.y + bounds.size.y, z);
			RaycastHit hit;

			if (Physics.Raycast (position, Vector3.down, out hit, bounds.size.y * 2)) {
				position.y = hit.point.y;

				if (isLocationInConvex (convexHull, position) && isLocationAtAnotherCar(position)) {
					return position + new Vector3(0.0f,1.0f,0.0f);
				}
			}
		}
		return Vector3.zero;
	}

	public static bool isLocationAtAnotherCar(Vector3 location){
		CarController[] cars = GameObject.FindObjectsOfType<CarController> ();
		foreach (CarController car in cars) {
			float size = Vector3.Magnitude(car.GetComponentInParent<MeshCollider> ().bounds.extents);
			if (Vector3.Distance (car.transform.position, location) < size) {
				return true;
			}
		}
		return false;
	}

	public static Vector3[] MinimizeConvexHull(Vector3[] convexHull){
		Vector3 average = findAverage (convexHull);
		Vector3[] result = new Vector3[convexHull.Length];
		for (int i = 0; i < convexHull.Length; i++) {
			result [i] = MinimizeLine (average, convexHull [i], 0.4f);
		}
		return result;
	}

	public static Vector3 MinimizeLine (Vector3 center, Vector3 point, float percentage){
		return (point - center) * percentage + center;
	}

	public static Vector3 findAverage(Vector3[] list){
		Vector3 sum = Vector3.zero;
		for(int i =0; i < list.Length; i++){
			sum += list [i];
		}
		return sum / list.Length;
	}

	public static bool isLocationInConvex(Vector3[] convexHull, Vector3 location){
		for(int i = 0; i < convexHull.Length; i++){
			int j = (i +1) % convexHull.Length;
			if (isLeft (convexHull[i],convexHull[j],location)) {
				return false;
			}
		}
		return true;
	}

	public static bool isLeft(Vector3 a, Vector3 b, Vector3 point){
		return ((b.x - a.x)*(point.z - a.z) - (b.z - a.z)*(point.x - a.x)) > 0;
	}
		
	public static void SetCarMaterialColoursFromHue (Material[] materials, float hue) {
		materials [0].color = Color.black; // Spoiler
		materials [1].color = Color.HSVToRGB(hue/360f, 0.96f, 0.67f); // Side glow
		materials [2].color = Color.HSVToRGB(hue/360f, 0.96f, 0.67f); // Blades
		materials [3].color = Color.HSVToRGB (hue / 360f, 1f, 0.38f); // Body
		materials [4].color = Color.gray; // Blades Inner
		materials [5].color = Color.black; // Winscreen
	}
}


