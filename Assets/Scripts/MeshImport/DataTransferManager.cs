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

				Material material = Resources.Load("Materials/MeshDefault", typeof(Material)) as Material;

				mesh = FastObjImporter.Instance.ImportString(data);

				mesh.RecalculateBounds();
				;
				Debug.Log("creating gameobject");
				GameObject moduleGo = new GameObject(); //create the game object
				MeshFilter filter = moduleGo.AddComponent<MeshFilter>(); //add the meshfilter
				filter.mesh = mesh; //assign the mesh
				moduleGo.AddComponent<MeshRenderer>(); //and mesh renderer

				GameObject root = GameObject.Find("Marker scene");
				Debug.Log (material);
				Debug.Log("adding gameobject");
				moduleGo.transform.parent = root.transform;


				MeshRenderer meshRenderer = moduleGo.GetComponent<MeshRenderer>();
				meshRenderer.material = material;


				SetLayerRecursively(moduleGo, 9);


			});
	}

	void handleMarkers(string data) {
		
		UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				string filename = System.IO.Path.Combine(Application.persistentDataPath, "markers.dat");

				using(TextWriter writer = new StreamWriter(filename))
				{
					writer.Write(data);
					writer.Close();
				}

				GameObject toolkit = GameObject.Find("ARToolKit");

				ARMarker marker = toolkit.GetComponent<ARMarker>();

				marker.enabled = false;

				marker.MarkerType = MarkerType.Multimarker;
				marker.MultiConfigFile = "markers.dat";
				marker.MultiConfigNonStreamingFile = filename;

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
		