using System;
using UnityEngine;
using System.Collections.Generic;

public class RelocateTest : MonoBehaviour
{

	GameMapObjects gameMapObjects;
	TestMeshImporter meshTransferManger;
	public GameObject Car;

	void Start ()
	{
		var ugly = UnityThreadHelper.Dispatcher;

		meshTransferManger = new TestMeshImporter ();
		meshTransferManger.FetchData ("localhost", 3111);
		meshTransferManger.OnMeshDataReceivedEvent += () => {
			UnityThreadHelper.Dispatcher.Dispatch (() => {
				gameMapObjects = meshTransferManger.ProduceGameObjects ();
				RelocateCar();
			});
		};
	}

	void RelocateCar(){
		GameObject obj = GameObject.Instantiate(Car);
		Vector3 pos = GameUtils.FindSpawnLocationInsideConvexHull (gameMapObjects);
		obj.transform.position = pos;

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

}

