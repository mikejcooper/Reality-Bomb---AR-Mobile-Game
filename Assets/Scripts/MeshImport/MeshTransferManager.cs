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

	private static string PATTERN_FILE_NAME = "markers.dat";
	private static string PATTERN_FILE_PATH = System.IO.Path.Combine (Application.persistentDataPath, PATTERN_FILE_NAME);

	private WebSocket _ws;

	private bool _meshReceived;
	private bool _markersReceived;
	private bool _chullVerticesReceived;
	private bool _boundaryVerticesReceived;

	private Vector3[] _meshVertices;
	private Vector3[] _meshNormals;
	private Vector2[] _meshUVs;
	private int[] _meshTriangles;

	private List<Vector3> _convexHullVertices;
	private List<Vector3> _boundaryVertices;

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

		// reset flags
		_meshReceived = _markersReceived = _chullVerticesReceived = _boundaryVerticesReceived = false;

		Debug.Log ("initing websocket");
		_ws = new WebSocket (url);

		_ws.OnMessage += (sender, e) => {
			if (e.IsText) {
				if (e.Data.StartsWith ("mesh")) {
					string data = e.Data.Substring ("mesh".Length);
					FastObjImporter.Instance.ImportString (data, ref _meshVertices, ref _meshNormals, ref _meshUVs, ref _meshTriangles);
					_meshReceived = true;
					TryToCallback ();
				} else if (e.Data.StartsWith ("markers")) {
					SaveMarkerData (e.Data.Substring ("markers".Length));
				} else if (e.Data.StartsWith ("chull_vertices")) {
					_convexHullVertices = ParseVertices(e.Data.Substring("chull_vertices".Length));
					_convexHullVertices = InvertVerticesX(_convexHullVertices);
					_chullVerticesReceived = true;
					TryToCallback ();

				} else if (e.Data.StartsWith ("boundary_vertices")) {
					_boundaryVertices = ParseVertices(e.Data.Substring("boundary_vertices".Length));
					_boundaryVertices = InvertVerticesX(_boundaryVertices);
					_boundaryVerticesReceived = true;
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
		if (_meshReceived && _markersReceived && _chullVerticesReceived && _boundaryVerticesReceived) { //_isPatternDataSaved && _triangles.Length > 0 && _boundaryVertices.Count > 0 && _convexHullVertices.Count > 0) {
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


		Material wireframeMaterial = Resources.Load ("Materials/Wireframe", typeof(Material)) as Material;
		GameObject ground = ProduceGameObjectFromMesh (groundMesh, wireframeMaterial);
		Material boundaryMaterial = Resources.Load ("Materials/Boundary", typeof(Material)) as Material;
		GameObject boundary = ProduceGameObjectFromMesh (boundaryMesh, boundaryMaterial);

		return new GameMapObjects(ground,boundary,_convexHullVertices);
	}


			
	public GameObject ProduceGameObjectFromMesh(Mesh mesh, Material material){

		PhysicMaterial physicMaterial = Resources.Load ("Materials/BouncyMaterial", typeof(PhysicMaterial)) as PhysicMaterial;

		GameObject MeshObject = new GameObject ("World Mesh");

		MeshFilter filter = MeshObject.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = MeshObject.AddComponent<MeshFilter> ();

		filter.mesh = mesh;

		MeshRenderer renderer = MeshObject.GetComponent<MeshRenderer> ();
		if (renderer == null)
			renderer = MeshObject.AddComponent<MeshRenderer> ();

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

	Mesh getGroundMesh(){
		Mesh mesh = new Mesh ();
		mesh.vertices = _meshVertices;
		mesh.normals = _meshNormals;
		mesh.uv = _meshUVs;
		mesh.triangles = _meshTriangles;

		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		return mesh;
	}

	Mesh getBoundaryMesh(List<Vector3> positions){
		Mesh mesh = new Mesh ();
		Vector3[] vertices = new Vector3[positions.Count * 2];
		int[] triangles = new int[positions.Count * 4 * 3];
		mesh.name = "ScriptedMesh";
		for (int i = 0; i < positions.Count; i++) {
			vertices [i]                   = new Vector3 (positions[i].x , positions[i].y, positions[i].z);
			vertices [i + positions.Count] = new Vector3 (positions[i].x , positions[i].y+5.0f , positions[i].z);

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

		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		return mesh;
	}

	private void SaveMarkerData (string data) {
		UnityThreadHelper.Dispatcher.Dispatch (() => {
			
			// save to the file, we can't pass the file by string to ARToolKit
			// because ARToolKit calls a native method that processes the file
			// based off a file path. So we must save to a file, to pass a path
			using (TextWriter writer = new StreamWriter (PATTERN_FILE_PATH)) {
				writer.Write (data);
				writer.Close ();
			}

			_markersReceived = true;

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
