using UnityEngine;
using System.Collections;
using WebSocketSharp;
using System.IO;

public class DataTransferManager : MonoBehaviour {

	WebSocket ws;
	Mesh mesh;


	public static void SetLayerRecursively(GameObject go, int layerNumber)
	{
		foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layerNumber;
		}
	}

	// Use this for initialization
	void Start () {
		
		// singleton needs to be started here on Main thread
		var ugly = UnityThreadHelper.Dispatcher;


		ws = new WebSocket ("ws://localhost:3110");

		ws.OnMessage += (sender, e) => {
			if (e.IsText) {
				Debug.Log("received message");

				if (e.Data.StartsWith("mesh")) {
					handleMesh(e.Data.Substring(4));
				} else if (e.Data.StartsWith("markers")) {
					handleMarkers(e.Data.Substring(7));
				} else {
					Debug.Log("unknwon websocket event");
				}

			}
		};

		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log("websocket closed");
			Invoke("connectWebsocket", 5);
		};

		ws.OnError += (object sender, WebSocketSharp.ErrorEventArgs e) => {
			Debug.Log("websocket error");
			Invoke("connectWebsocket", 5);
		};


		connectWebsocket ();
	}

	void connectWebsocket () {
		ws.Connect ();
	}

	void handleMesh (string data) {
		UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				// choose the material - we can get round to using a custom invisible
				// shader at some point here, but for development purposes it's nice
				// to be able to see the mesh
				Material material = Resources.Load("Materials/MeshDefault", typeof(Material)) as Material;

				// convert the mesh object string into an actual Unity mesh
				mesh = FastObjImporter.Instance.ImportString(data);
				mesh.RecalculateBounds();

				GameObject worldMesh = new GameObject("world mesh");
				MeshFilter filter = worldMesh.AddComponent<MeshFilter>();
				filter.mesh = mesh;
				worldMesh.AddComponent<MeshRenderer>();

				// attach to Marker scene
				GameObject root = GameObject.Find("Marker scene");
				worldMesh.transform.parent = root.transform;

				// set mesh material
				MeshRenderer meshRenderer = worldMesh.GetComponent<MeshRenderer>();
				meshRenderer.material = material;

				// set visibility based on whether or not the marker is currently visible
				GameObject toolkit = GameObject.Find("ARToolKit");
				ARMarker marker = toolkit.GetComponent<ARMarker>();
				worldMesh.SetActive(marker.Visible);

				// assign to correct layer for ArToolKit
				SetLayerRecursively(worldMesh, 9);


			});
	}

	void handleMarkers(string data) {
		
		UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				// find the path we'll store the marker file under
				string filename = "markers.dat";
				string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);

				// save to the file, we can't pass the file by string to ARToolKit
				// because ARToolKit calls a native method that processes the file
				// based off a file path. So we must save to a file, to pass a path
				using(TextWriter writer = new StreamWriter(filepath))
				{
					writer.Write(data);
					writer.Close();
				}

				// find the ARToolKit object and associated marker component
				GameObject toolkit = GameObject.Find("ARToolKit");
				ARMarker marker = toolkit.GetComponent<ARMarker>();

				// temporarily disable any existing instance
				marker.enabled = false;

				// use our custom field to avoid the file being loaded from
				// StreamingAssets by ARToolKit
				marker.MarkerType = MarkerType.Multimarker;
				marker.MultiConfigFile = filename;
				marker.MultiConfigNonStreamingFile = filepath;

				// enable the marker so that ARToolKit knows params have changed
				marker.enabled = true;
			});
	}

	void onApplicationQuit () {
		ws.Close ();
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
		