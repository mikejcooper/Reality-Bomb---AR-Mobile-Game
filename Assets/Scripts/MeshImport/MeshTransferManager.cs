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
	private static List<Vector3> _convexHullVertices;
	private static List<Vector3> _boundaryVertices;
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
					string data = e.Data.Substring ("mesh".Length);
					FastObjImporter.Instance.ImportString (data, ref _vertices, ref _normals, ref _uvs, ref _triangles);
					TryToCallback ();
				} else if (e.Data.StartsWith ("markers")) {
					SaveMarkerData (e.Data.Substring ("markers".Length));
				} else if (e.Data.StartsWith ("chull_vertices")) {
					_convexHullVertices = ParseVertices(e.Data.Substring("chull_vertices".Length));
					_convexHullVertices = InvertVerticesX(_convexHullVertices);
					TryToCallback ();
				} else if (e.Data.StartsWith ("boundary_vertices")) {
					_boundaryVertices = ParseVertices(e.Data.Substring("boundary_vertices".Length));
					_boundaryVertices = InvertVerticesX(_boundaryVertices);
					TryToCallback ();
				} else {
					Debug.Log ("unknown websocket event: " + e.Data);
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
		if (_isPatternDataSaved && _triangles.Length > 0 && _boundaryVertices.Count > 0 && _convexHullVertices.Count > 0) {
			UnityThreadHelper.Dispatcher.Dispatch (() => {
				if (OnMeshDataReceivedEvent != null)
					OnMeshDataReceivedEvent ();
			}
			);
			_ws.Close ();
		}
	}

	public GameMapObjects ProduceGameObjects () {
		Mesh boundaryMesh = getBoundaryMesh (_boundaryVertices);
		Mesh groundMesh = getGroundMesh ();


		GameObject ground = ProduceGameObjectFromMesh (groundMesh);
		GameObject boundary = ProduceGameObjectFromMesh (boundaryMesh);

		return new GameMapObjects(ground,boundary,_convexHullVertices);
	}

	Mesh getGroundMesh(){
		Mesh mesh = new Mesh ();
		mesh.vertices = _vertices;
		mesh.normals = _normals;
		mesh.uv = _uvs;
		mesh.triangles = _triangles;

		mesh.RecalculateBounds ();
		return mesh;
	}
		
	public GameObject ProduceGameObjectFromMesh(Mesh mesh){
		
		mesh.RecalculateNormals();

		Material material = Resources.Load ("Materials/MeshVisible", typeof(Material)) as Material;
		PhysicMaterial physicMaterial = Resources.Load ("Materials/BouncyMaterial", typeof(PhysicMaterial)) as PhysicMaterial;

		GameObject MeshObject = new GameObject ("World Mesh");

		MeshFilter filter = MeshObject.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = MeshObject.AddComponent<MeshFilter> ();

		filter.mesh = mesh;

		MeshRenderer renderer = MeshObject.GetComponent<MeshRenderer> ();
		if (renderer == null)
			renderer = MeshObject.AddComponent<MeshRenderer> ();

		if (Application.isMobilePlatform) {
			renderer.enabled = false;
		}

		MeshCollider collider = MeshObject.GetComponent<MeshCollider> ();
		if (collider == null)
			collider = MeshObject.AddComponent<MeshCollider> ();

		collider.sharedMesh = mesh;
		collider.material = physicMaterial;

		// set mesh material
		renderer.material = material;

		// set the layer to ground
		SetLayerRecursively (MeshObject, 8);

		return MeshObject;
	}

	Mesh getBoundaryMesh(List<Vector3> positions){
		Mesh mesh = new Mesh ();
		Vector3[] vertices = new Vector3[positions.Count * 2];
		int[] triangles = new int[positions.Count * 4 * 3];
		mesh.name = "ScriptedMesh";
		for (int i = 0; i < positions.Count; i++) {
			vertices [i]                   = new Vector3 (positions[i].x , -0.2f, positions[i].z);
			vertices [i + positions.Count] = new Vector3 (positions[i].x , 2.0f , positions[i].z);

			triangles [0 * 3 * positions.Count + 3 * i + 0] = i;
			triangles [0 * 3 * positions.Count + 3 * i + 1] = (i + 1) % positions.Count;
			triangles [0 * 3 * positions.Count + 3 * i + 2] = (i + 1) % positions.Count + positions.Count;

			triangles [1 * 3 * positions.Count + 3 * i + 0] = i;
			triangles [1 * 3 * positions.Count + 3 * i + 1] = (i + 1) % positions.Count + positions.Count;
			triangles [1 * 3 * positions.Count + 3 * i + 2] = positions.Count + i;

			triangles [2 * 3 * positions.Count + 3 * i + 0] = (i + 1) % positions.Count + positions.Count;
			triangles [2 * 3 * positions.Count + 3 * i + 1] = (i + 1) % positions.Count;
			triangles [2 * 3 * positions.Count + 3 * i + 2] = i;

			triangles [3 * 3 * positions.Count + 3 * i + 0] = positions.Count + i;
			triangles [3 * 3 * positions.Count + 3 * i + 1] = (i + 1) % positions.Count + positions.Count;
			triangles [3 * 3 * positions.Count + 3 * i + 2] = i;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();


		return mesh;
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

	List<Vector3> ParseVertices (string data) {
		string[] lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		List<Vector3> vertices = new List<Vector3> ();
		foreach (string vertex in lines) {
			
			if (vertex.Trim ().Length == 0)
				continue;
			
			string[] values = vertex.Split(',');
			var vec = new Vector3 (Convert.ToSingle(values [0]), Convert.ToSingle(values [1]), Convert.ToSingle(values [2]));
			vertices.Add (vec);
		}

		return vertices;
	}

	private static List<Vector3> InvertVerticesX(List<Vector3> vertices){
		List<Vector3> result = new List<Vector3> ();
		foreach(Vector3 vertex in vertices){
			result.Add (new Vector3 (-vertex.x, vertex.y, vertex.z));
		}
		return result;
	}

}
