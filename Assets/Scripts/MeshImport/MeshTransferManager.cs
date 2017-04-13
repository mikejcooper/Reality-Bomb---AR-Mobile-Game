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
	private static List<Vector3> _markerVertices;
	private bool _isPatternDataSaved = false;
	private static string PATTERN_FILE_NAME = "markers.dat";
	private static string PATTERN_FILE_PATH = System.IO.Path.Combine (Application.persistentDataPath, PATTERN_FILE_NAME);

	public static List<Vector3> MarkerVertices
	{
		get { return _markerVertices; }
	}

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
				} else if (e.Data.StartsWith ("vertices")) {
					handleMarkerCoords(e.Data.Substring("vertices".Length));
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
		if (_isPatternDataSaved && _triangles.Length > 0 && _markerVertices.Count > 0) {
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

		if (Application.isMobilePlatform) {
			renderer.enabled = false;
		}

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

	void handleMarkerCoords (string data) {
		string[] lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		_markerVertices = new List<Vector3> ();
		foreach (string vertex in lines) {
			
			if (vertex.Trim ().Length == 0)
				continue;
			
			string[] values = vertex.Split(',');
			var vec = new Vector3 (Convert.ToSingle(values [0]), Convert.ToSingle(values [1]), Convert.ToSingle(values [2]));
			_markerVertices.Add (vec);
		}

		TryToCallback ();
	}

}
