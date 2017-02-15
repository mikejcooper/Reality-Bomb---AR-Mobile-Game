using UnityEngine;
using System.Collections;
using WebSocketSharp;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Net.Sockets;
using System.Text;
using UnityThreading;
using UnityEngine.Networking;

public class DataTransferManager {

	public delegate void OnMeshDataReceived ();

	public event OnMeshDataReceived OnMeshDataReceivedEvent;

	public string MeshData;

	private WebSocket _ws;


//	public static GameObject s_WorldMesh;


	public static void SetLayerRecursively(GameObject go, int layerNumber)
	{
		foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layerNumber;
		}
	}

	public void fetchData (string address, int port) {

		var url = "ws://" + address + ":" + port.ToString ();

		if (_ws != null && _ws.Url.Host == address) {
			Debug.Log ("existing ws: " + _ws.Url.Host);
			return;
		}
		_ws = new WebSocket (url);

		_ws.OnMessage += (sender, e) => {
			if (e.IsText) {
				Debug.Log("received message");
				UnityThreadHelper.Dispatcher.Dispatch(() =>
					{
						if (e.Data.StartsWith("mesh")) {
							MeshData = e.Data.Substring(4);
							if (OnMeshDataReceivedEvent != null)
								OnMeshDataReceivedEvent ();
		//					handleMesh(e.Data.Substring(4));
						} else if (e.Data.StartsWith("markers")) {
		//					handleMarkers(e.Data.Substring(7));
						} else if (e.Data.StartsWith("triangles")) {
		//					handleTrianglesMesh(e.Data.Substring(9));
						} else {
							Debug.Log("unknwon websocket event: "+e.Data);
						}
					}
				);

			}
		};

		_ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log("websocket closed");
//			Invoke("connectWebsocket", 5);
		};

		_ws.OnError += (object sender, WebSocketSharp.ErrorEventArgs e) => {
			Debug.Log("websocket error");
//			Invoke("connectWebsocket", 5);
		};

		UnityThreadHelper.Dispatcher.Dispatch (() => {
			_ws.Connect ();
		});
	}

	public void produceMeshObject (GameObject gameObject, string meshData) {
		// choose the material - we can get round to using a custom invisible
		// shader at some point here, but for development purposes it's nice
		// to be able to see the mesh
		Material material = Resources.Load("Materials/MeshDefault", typeof(Material)) as Material;

		// convert the mesh object string into an actual Unity mesh
		Mesh mesh = FastObjImporter.Instance.ImportString(meshData);
		mesh.RecalculateBounds();

		MeshFilter filter = gameObject.GetComponent<MeshFilter> ();
		if (filter == null) filter = gameObject.AddComponent<MeshFilter> ();

		filter.mesh = mesh;

		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer> ();
		if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer> ();

		MeshCollider collider = gameObject.GetComponent<MeshCollider> ();
		if (collider == null) collider = gameObject.AddComponent<MeshCollider> ();

		collider.sharedMesh = mesh;

		// attach to Marker scene
		GameObject root = GameObject.Find("Marker scene");
		gameObject.transform.parent = root.transform;

		// set mesh material
		renderer.material = material;

		// assign to correct layer for ArToolKit
		SetLayerRecursively(gameObject, 9);

		// add network identity so that it's propagated
		//NetworkIdentity networkIdentity = s_WorldMesh.AddComponent<NetworkIdentity>();

		// propagate mesh across clients (TODO)
		//NetworkServer.Spawn(s_WorldMesh);

		// Reposition with delay.
		//TODO: Change this to use events or a callback

		//Invoke("Reposition", 2);
//		GameManager.s_Instance.RepositionAllCars();
	}


//	void handleMarkers(string data) {
//		Debug.Log ("received markers");
//		UnityThreadHelper.Dispatcher.Dispatch(() =>
//			{
//				// find the path we'll store the marker file under
//				string filename = "markers.dat";
//				string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
//
//				// save to the file, we can't pass the file by string to ARToolKit
//				// because ARToolKit calls a native method that processes the file
//				// based off a file path. So we must save to a file, to pass a path
//				using(TextWriter writer = new StreamWriter(filepath))
//				{
//					writer.Write(data);
//					writer.Close();
//				}
//
//				// find the ARToolKit object and associated marker component
//				GameObject toolkit = GameObject.Find("ARToolKit");
//				ARMarker marker = toolkit.GetComponent<ARMarker>();
//
//				// temporarily disable any existing instance
//				marker.enabled = false;
//
//				// use our custom field to avoid the file being loaded from
//				// StreamingAssets by ARToolKit
//				marker.MarkerType = MarkerType.Multimarker;
//				marker.MultiConfigFile = filename;
//				marker.MultiConfigNonStreamingFile = filepath;
//
//				// enable the marker so that ARToolKit knows params have changed
//				marker.enabled = true;
//			});
//	}
//
//	//This function spawns the objects that will 'mask the markers in the scene. It will overlay the rigidbody over the markers
//	void handleTrianglesMesh(string data){
//		UnityThreadHelper.Dispatcher.Dispatch(() =>
//			{
//				Mesh mesh = MarkerFileParser.Instance.ImportString(data);
//				List<Transform> transforms = getTransformOfTriangles (mesh);
//				List<float> markerSizes = getMarkerSizes (mesh);
//				int numberOfMarkers = transforms.Count;
//				for (int i = 0; i < numberOfMarkers; i++){
//					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//					cube.transform.position = transforms[i].transform.position;
//					cube.transform.rotation = transforms[i].transform.rotation;
//
//					cube.transform.position = new Vector3(transforms[i].transform.position.x, transforms[i].transform.position.y-markerSizes[i]/2.5f, transforms[i].transform.position.z);
//
//					cube.transform.localScale = new Vector3 (markerSizes[i],markerSizes[i],markerSizes[i]);
//
//
//					cube.AddComponent<Obscurable>();
//					//
//					SetLayerRecursively(cube, 9);
//
//				}
//			});
//	}
//
//	List<Transform> getTransformOfTriangles(Mesh mesh) {
//		List<Transform> markerLocations = new List<Transform> ();
//		Debug.Log (mesh.triangles.Length);
//		for (int i = 0; i < mesh.triangles.Length; i += 3)
//		{
//			Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
//			Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
//			Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];
//
//			GameObject gameObject = new GameObject ();
//			gameObject.transform.position = new Vector3 ((p1.x + p3.x) / 2, (p1.y + p3.y) / 2, (p1.z + p3.z) / 2);
//			gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross (p2 - p1, p2 - p3).normalized, p2-p3);
//			markerLocations.Add (gameObject.transform);
//		}
//		return markerLocations;
//	}
//
//	List<float> getMarkerSizes(Mesh mesh) {
//		List<float> sizes = new List<float> ();
//		for (int i = 0; i < mesh.triangles.Length; i += 3)
//		{
//			Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
//			Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
//			sizes.Add (Vector3.Distance (p1,p2));
//		}
//		return sizes;
//	}
//
//	void onApplicationQuit () {
//		ws.Close ();
//	}
//
//
//	// Update is called once per frame
//	void Update () {
//
//	}

}
