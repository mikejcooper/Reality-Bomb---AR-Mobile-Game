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
					_chullVerticesReceived = true;
					TryToCallback ();
				} else if (e.Data.StartsWith ("boundary_vertices")) {
					_boundaryVertices = ParseVertices(e.Data.Substring("boundary_vertices".Length));
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

	public GameObject ProduceGameObject () {

		Mesh mesh = new Mesh ();
		mesh.vertices = _meshVertices;
		mesh.normals = _meshNormals;
		mesh.uv = _meshUVs;
		mesh.triangles = _meshTriangles;

		mesh.RecalculateBounds ();
		mesh.RecalculateNormals();

		// choose the material - we can get round to using a custom invisible
		// shader at some point here, but for development purposes it's nice
		// to be able to see the mesh
		Material material = Resources.Load ("Materials/Wireframe", typeof(Material)) as Material;
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

		// set the layer to ground
		SetLayerRecursively (worldMeshObj, 8);

		return worldMeshObj;
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

}
