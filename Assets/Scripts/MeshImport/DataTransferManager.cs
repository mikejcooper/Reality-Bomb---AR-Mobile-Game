using UnityEngine;
using System.Collections;
using WebSocketSharp;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityThreading;
using UnityEngine.Networking;

public class DataTransferManager : NetworkBehaviour {

	WebSocket ws;
	Mesh mesh;

	public static GameObject s_WorldMesh;


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

        UnityThreadHelper.CreateThread(ListenForBroadcasts);
        //UnityThreading.ActionThread myThread =

    }

	void ListenForBroadcasts()
	{
		int port = 3110;

		try {
			var client = new UdpClient (port);

			while (true) {
				try {
					IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
					byte[] data = client.Receive (ref anyIP);

					string text = Encoding.UTF8.GetString (data);

					if (text == "RealityBomb") {
						// we've found our server
						connectWebsocket (anyIP.Address.ToString (), port + 1);
					}

				} catch (Exception err) {
					print (err.ToString ());
				}

			}
		

		} catch (SocketException e) {
			Debug.Log ("broadcast socket is already in use, assuming broadcast is coming from localhost");
            Debug.Log(e);
			connectWebsocket ("localhost", port + 1);
		}

	}



	void connectWebsocket (string address, int port) {

		var url = "ws://" + address + ":" + port.ToString ();

		if (ws != null && ws.Url.Host == address) {
			Debug.Log ("existing ws: " + ws.Url.Host);
			return;
		}
		ws = new WebSocket (url);

		ws.OnMessage += (sender, e) => {
			if (e.IsText) {
				Debug.Log("received message");

				if (e.Data.StartsWith("mesh")) {
					handleMesh(e.Data.Substring(4));
				} else if (e.Data.StartsWith("markers")) {
					handleMarkers(e.Data.Substring(7));
				} else if (e.Data.StartsWith("triangles")) {
					handleTrianglesMesh(e.Data.Substring(9));
				} else {
					Debug.Log("unknwon websocket event: "+e.Data);
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

		UnityThreadHelper.Dispatcher.Dispatch (() => {
			ws.Connect ();
		});
	}

    void handleMesh(string data)
    {
        UnityThreadHelper.Dispatcher.Dispatch(() =>
            {
                // choose the material - we can get round to using a custom invisible
                // shader at some point here, but for development purposes it's nice
                // to be able to see the mesh
                Material material = Resources.Load("Materials/MeshDefault", typeof(Material)) as Material;

                // convert the mesh object string into an actual Unity mesh
                mesh = FastObjImporter.Instance.ImportString(data);
                mesh.RecalculateBounds();

                s_WorldMesh = new GameObject("world mesh");

                MeshFilter filter = s_WorldMesh.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                s_WorldMesh.AddComponent<MeshRenderer>();

                MeshCollider collider = s_WorldMesh.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;

                // attach to Marker scene
                GameObject root = GameObject.Find("Marker scene");
                s_WorldMesh.transform.parent = root.transform;

                // set mesh material
                MeshRenderer meshRenderer = s_WorldMesh.GetComponent<MeshRenderer>();
                meshRenderer.material = material;

                // assign to correct layer for ArToolKit
                SetLayerRecursively(s_WorldMesh, 9);

                // add network identity so that it's propagated
                //NetworkIdentity networkIdentity = s_WorldMesh.AddComponent<NetworkIdentity>();

                // propagate mesh across clients (TODO)
                //NetworkServer.Spawn(s_WorldMesh);

                // Reposition with delay.
                //TODO: Change this to use events or a callback
                Invoke("Reposition", 2);
            });
    }

    void Reposition()
    {
        PTBGameManager.s_Instance.RepositionAllCars();
    }


	void handleMarkers(string data) {
		Debug.Log ("received markers");
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

	//This function spawns the objects that will 'mask the markers in the scene. It will overlay the rigidbody over the markers
	void handleTrianglesMesh(string data){
		UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				Mesh mesh = MarkerFileParser.Instance.ImportString(data);
				List<Transform> transforms = getTransformOfTriangles (mesh);
				List<float> markerSizes = getMarkerSizes (mesh);
				int numberOfMarkers = transforms.Count;
				for (int i = 0; i < numberOfMarkers; i++){
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = transforms[i].transform.position;
					cube.transform.rotation = transforms[i].transform.rotation;

					cube.transform.position = new Vector3(transforms[i].transform.position.x, transforms[i].transform.position.y-markerSizes[i]/2.5f, transforms[i].transform.position.z);

					cube.transform.localScale = new Vector3 (markerSizes[i],markerSizes[i],markerSizes[i]);


					cube.AddComponent<Obscurable>();
//
					SetLayerRecursively(cube, 9);

				}
			});
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

	void onApplicationQuit () {
		ws.Close ();
	}

	
	// Update is called once per frame
	void Update () {
	
	}

}
		