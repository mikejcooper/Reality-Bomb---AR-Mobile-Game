using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class InitializeScene : MonoBehaviour {

	public Rigidbody mBlock;  

	// Use this for initialization
	void Start () {
		SpawnMarkerMasks (mBlock,"Assets/meshes/unity_markers.obj");
	}

	//This function spawns the objects that will 'mask the markers in the scene. It will overlay the rigidbody over the markers
	void SpawnMarkerMasks(Rigidbody rigidBody, string FileName){
		ObjImporter objectImporter = new ObjImporter ();
		Mesh mesh = objectImporter.ImportFile (FileName);
		List<Transform> transforms = getTransformOfTriangles (mesh);
		foreach (var transform in transforms) {
			Rigidbody body;
			body = Instantiate (rigidBody, transform.position,transform.rotation) as Rigidbody;
		}
	}

	List<Transform> getTransformOfTriangles(Mesh mesh) {
		List<Transform> markerLocations = new List<Transform> ();
		Debug.Log (mesh.triangles.Length);
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
			Vector3 p2 = mesh.vertices[mesh.triangles[i + 2]];
			GameObject gameObject = new GameObject ();
			gameObject.transform.position = new Vector3 ((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
			gameObject.transform.rotation = Quaternion.FromToRotation (p1, p2);
			markerLocations.Add (gameObject.transform);
		}
		return markerLocations;
	}
		


	
	// Update is called once per frame
	void Update () {
		
	}
}
