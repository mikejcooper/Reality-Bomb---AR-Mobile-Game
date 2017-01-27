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
		List<float> markerSizes = getMarkerSizes (mesh);
		int numberOfMarkers = transforms.Count;
		for (int i = 0; i < numberOfMarkers; i++){
			Rigidbody body;
			body = Instantiate (rigidBody, transforms[i].transform.position,transforms[i].transform.rotation) as Rigidbody;
			body.transform.localScale = new Vector3 (markerSizes[i],markerSizes[i],markerSizes[i]);
		}
	}

	List<Transform> getTransformOfTriangles(Mesh mesh) {
		List<Transform> markerLocations = new List<Transform> ();
		Debug.Log (mesh.triangles.Length);
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
			Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
			Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];

			GameObject gameObject = new GameObject ();
			gameObject.transform.position = new Vector3 ((p1.x + p3.x) / 2, (p1.y + p3.y) / 2, (p1.z + p3.z) / 2);
			gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross (p2 - p1, p2 - p3).normalized, p2-p3);
			markerLocations.Add (gameObject.transform);
		}
		return markerLocations;
	}

	List<float> getMarkerSizes(Mesh mesh) {
		List<float> sizes = new List<float> ();
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
			Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
			sizes.Add (Vector3.Distance (p1,p2));
		}
		return sizes;
	}
		


	
	// Update is called once per frame
	void Update () {
		
	}
}
