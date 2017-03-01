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

public class MeshTransferManager {

	public delegate void OnMeshDataReceived ();

	public event OnMeshDataReceived OnMeshDataReceivedEvent;

	private WebSocket _ws;

	private Vector3[] _vertices;
	private Vector3[] _normals;
	private Vector2[] _uvs;
	private int[] _triangles;
	private bool _isPatternDataSaved = false;
	private static string PATTERN_FILE_NAME = "markers.dat";
	private static string PATTERN_FILE_PATH = System.IO.Path.Combine (Application.persistentDataPath, PATTERN_FILE_NAME);


	private static void SetLayerRecursively (GameObject go, int layerNumber) {
		foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
			trans.gameObject.layer = layerNumber;
		}
	}

	public void FetchData (string address, int port) {

		Debug.Log (string.Format ("FetchData address: {0}, port: {1}", address, port));
		var url = "ws://" + address + ":" + port.ToString ();

		if (_ws != null && _ws.IsAlive && _ws.Url.Host == address) {
			Debug.Log ("existing ws: " + _ws.Url.Host);
			return;
		}
		Debug.Log ("initing websocket");
		_ws = new WebSocket (url);

		_ws.OnMessage += (sender, e) => {
			if (e.IsText) {
				if (e.Data.StartsWith ("mesh")) {
					string data = e.Data.Substring (4);
					FastObjImporter.Instance.ImportString (data, ref _vertices, ref _normals, ref _uvs, ref _triangles);
					TryToCallback ();
				} else if (e.Data.StartsWith ("markers")) {
					SaveMarkerData (e.Data.Substring (7));
				} else if (e.Data.StartsWith ("triangles")) {
					//          handleTrianglesMesh(e.Data.Substring(9));
				} else {
					Debug.Log ("unknwon websocket event: " + e.Data);
				}
				

			}
		};

		_ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("websocket closed");
			//      Invoke("connectWebsocket", 5);
		};

		_ws.OnError += (object sender, WebSocketSharp.ErrorEventArgs e) => {
			Debug.Log ("websocket error");
			Debug.LogError (e.Exception);
			//      Invoke("connectWebsocket", 5);
		};

//		UnityThreadHelper.Dispatcher.Dispatch (() => {
		Debug.Log ("connecting websocket");
		_ws.Connect ();
//		});
	}

	private void TryToCallback () {
		if (_isPatternDataSaved && _triangles.Length > 0) {
			UnityThreadHelper.Dispatcher.Dispatch (() => {
				if (OnMeshDataReceivedEvent != null)
					OnMeshDataReceivedEvent ();
			}
			);
			_ws.Close ();
		}
	}

	public GameObject ProduceGameObject () {

		Mesh mesh = new Mesh ();
		mesh.vertices = _vertices;
		mesh.normals = _normals;
		mesh.uv = _uvs;
		mesh.triangles = _triangles;

		mesh.RecalculateBounds ();

		// choose the material - we can get round to using a custom invisible
		// shader at some point here, but for development purposes it's nice
		// to be able to see the mesh
		Material material = Resources.Load ("Materials/MeshVisible", typeof(Material)) as Material;
		PhysicMaterial physicMaterial = Resources.Load ("Materials/BouncyMaterial", typeof(PhysicMaterial)) as PhysicMaterial;

		GameObject worldMeshObj = new GameObject ("World Mesh");

		MeshFilter filter = worldMeshObj.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = worldMeshObj.AddComponent<MeshFilter> ();

		filter.mesh = mesh;

		MeshRenderer renderer = worldMeshObj.GetComponent<MeshRenderer> ();
		if (renderer == null)
			renderer = worldMeshObj.AddComponent<MeshRenderer> ();

		MeshCollider collider = worldMeshObj.GetComponent<MeshCollider> ();
		if (collider == null)
			collider = worldMeshObj.AddComponent<MeshCollider> ();

		collider.sharedMesh = mesh;
		collider.material = physicMaterial;

		// set mesh material
		renderer.material = material;

		// assign to correct layer for ArToolKit
		SetLayerRecursively (worldMeshObj, 9);

		return worldMeshObj;
	}


	private void SaveMarkerData (string data) {
		Debug.Log ("received markers");
		UnityThreadHelper.Dispatcher.Dispatch (() => {
			// find the path we'll store the marker file under



			// save to the file, we can't pass the file by string to ARToolKit
			// because ARToolKit calls a native method that processes the file
			// based off a file path. So we must save to a file, to pass a path
			using (TextWriter writer = new StreamWriter (PATTERN_FILE_PATH)) {
				writer.Write (data);
				writer.Close ();
			}

			_isPatternDataSaved = true;

			TryToCallback ();

	        
		});
	}

	public static void ApplyMarkerData (ARMarker markerComponent) {

		// temporarily disable any existing instance
		markerComponent.enabled = false;

		// use our custom field to avoid the file being loaded from
		// StreamingAssets by ARToolKit
		markerComponent.MarkerType = MarkerType.Multimarker;
		markerComponent.MultiConfigFile = PATTERN_FILE_NAME;
		markerComponent.MultiConfigNonStreamingFile = PATTERN_FILE_PATH;

		// enable the marker so that ARToolKit knows params have changed
		markerComponent.enabled = true;
	}


	//
	//  //This function spawns the objects that will 'mask the markers in the scene. It will overlay the rigidbody over the markers
	//  void handleTrianglesMesh(string data){
	//    UnityThreadHelper.Dispatcher.Dispatch(() =>
	//      {
	//        Mesh mesh = MarkerFileParser.Instance.ImportString(data);
	//        List<Transform> transforms = getTransformOfTriangles (mesh);
	//        List<float> markerSizes = getMarkerSizes (mesh);
	//        int numberOfMarkers = transforms.Count;
	//        for (int i = 0; i < numberOfMarkers; i++){
	//          GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
	//          cube.transform.position = transforms[i].transform.position;
	//          cube.transform.rotation = transforms[i].transform.rotation;
	//
	//          cube.transform.position = new Vector3(transforms[i].transform.position.x, transforms[i].transform.position.y-markerSizes[i]/2.5f, transforms[i].transform.position.z);
	//
	//          cube.transform.localScale = new Vector3 (markerSizes[i],markerSizes[i],markerSizes[i]);
	//
	//
	//          cube.AddComponent<Obscurable>();
	//          //
	//          SetLayerRecursively(cube, 9);
	//
	//        }
	//      });
	//  }
	//
	//  List<Transform> getTransformOfTriangles(Mesh mesh) {
	//    List<Transform> markerLocations = new List<Transform> ();
	//    Debug.Log (mesh.triangles.Length);
	//    for (int i = 0; i < mesh.triangles.Length; i += 3)
	//    {
	//      Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
	//      Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
	//      Vector3 p3 = mesh.vertices[mesh.triangles[i + 2]];
	//
	//      GameObject gameObject = new GameObject ();
	//      gameObject.transform.position = new Vector3 ((p1.x + p3.x) / 2, (p1.y + p3.y) / 2, (p1.z + p3.z) / 2);
	//      gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross (p2 - p1, p2 - p3).normalized, p2-p3);
	//      markerLocations.Add (gameObject.transform);
	//    }
	//    return markerLocations;
	//  }
	//
	//  List<float> getMarkerSizes(Mesh mesh) {
	//    List<float> sizes = new List<float> ();
	//    for (int i = 0; i < mesh.triangles.Length; i += 3)
	//    {
	//      Vector3 p1 = mesh.vertices[mesh.triangles[i + 0]];
	//      Vector3 p2 = mesh.vertices[mesh.triangles[i + 1]];
	//      sizes.Add (Vector3.Distance (p1,p2));
	//    }
	//    return sizes;
	//  }
	//
	//  void onApplicationQuit () {
	//    ws.Close ();
	//  }
	//
	//
	//  // Update is called once per frame
	//  void Update () {
	//
	//  }

}
