/* FastObjImporter.cs
 * by Marc Kusters (Nighteyes)
 * 
 * Used for loading .obj files exported by Blender
 * Example usage: Mesh myMesh = FastObjImporter.Instance.ImportFile("path_to_obj_file.obj");
 */

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class FastObjImporter
{

	#region singleton
	// Singleton code
	// Static can be called from anywhere without having to make an instance
	private static FastObjImporter _instance;

	// If called check if there is an instance, otherwise create it
	public static FastObjImporter Instance
	{
		get { return _instance ?? (_instance = new FastObjImporter()); }
	}
	#endregion

	private List<int> _triangles;
	private List<Vector3> _vertices;
	private List<Vector3> _normals;
	private List<int> _intArray;

	private const int MIN_POW_10 = -16;
	private const int MAX_POW_10 = 16;
	private const int NUM_POWS_10 = MAX_POW_10 - MIN_POW_10 + 1;
	private static readonly float[] pow10 = GenerateLookupTable();

	// Use this for initialization
	public Mesh ImportString(string data)
	{
		_triangles = new List<int>();
		_vertices = new List<Vector3>();
		_normals = new List<Vector3>();
		_intArray = new List<int>();

		LoadMeshData(data);

		Mesh mesh = new Mesh();

		mesh.vertices = _vertices.ToArray();
		mesh.uv = new Vector2[0];
		mesh.normals = _normals.ToArray();
		mesh.triangles = _triangles.ToArray();

		mesh.RecalculateBounds();
		;

		return mesh;
	}

	private void LoadMeshData(string data)
	{

		StringBuilder sb = new StringBuilder();
		string text = data;
		int start = 0;
		string objectName = null;
		int faceDataCount = 0;

		StringBuilder sbFloat = new StringBuilder();

		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\n')
			{
				sb.Remove(0, sb.Length);

				// Start +1 for whitespace '\n'
				sb.Append(text, start + 1, i - start);
				start = i;

				if (sb[0] == 'o' && sb[1] == ' ')
				{
					sbFloat.Remove(0, sbFloat.Length);
					int j = 2;
					while (j < sb.Length)
					{
						objectName += sb[j];
						j++;
					}
				}
				else if (sb[0] == 'v' && sb[1] == 'n' && sb[2] == ' ') // Normals
				{
					int splitStart = 3;

					Vector3 vec = new Vector3(GetFloat(sb, ref splitStart, ref sbFloat),
						GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat));
					_normals.Add (new Vector3 (-vec.z, -vec.y, vec.x));
				}
				else if (sb[0] == 'v' && sb[1] == ' ') // Vertices
				{
					int splitStart = 2;

					_vertices.Add(new Vector3(-GetFloat(sb, ref splitStart, ref sbFloat),
						GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat)));
				}
				else if (sb[0] == 'f' && sb[1] == ' ')
				{
					int splitStart = 2;

					int j = 1;
					_intArray.Clear();
					int info = 0;
					// Add faceData, a face can contain multiple triangles, facedata is stored in following order vert, uv, normal. If uv or normal are / set it to a 0
					while (splitStart < sb.Length && char.IsDigit(sb[splitStart]))
					{
						Vector3Int item = new Vector3Int(GetInt(sb, ref splitStart, ref sbFloat),
							GetInt(sb, ref splitStart, ref sbFloat), GetInt(sb, ref splitStart, ref sbFloat));
						j++;

						_intArray.Add(item.x);
						faceDataCount++;
					}

					info += j;
					j = 1;
					while (j + 2 < info) //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
					{
						_triangles.Add(_intArray[j+1]-1);
						_triangles.Add(_intArray[j]-1);
						_triangles.Add(_intArray[0]-1);

						j++;
					}
					//
					//          int splitStart = 2;
					//
					//          int p1 = GetInt (sb, ref splitStart, ref sbFloat);
					//          int p2 = GetInt (sb, ref splitStart, ref sbFloat);
					//          int p3 = GetInt (sb, ref splitStart, ref sbFloat);
					//
					//          triangles.Add (p3-1);
					//          triangles.Add (p2-1);
					//          triangles.Add (p1-1);


				}
			}
		}
	}

	private float GetFloat(StringBuilder sb, ref int start, ref StringBuilder sbFloat)
	{
		sbFloat.Remove(0, sbFloat.Length);
		bool valid = true; 
		while (start < sb.Length &&
			(char.IsDigit(sb[start]) || sb[start] == '-' || sb[start] == '.' || sb[start] == 'e'))
		{
			if (sb [start] == 'e')
				valid = false;

			sbFloat.Append(sb[start]);

			start++;
		}
		//    Debug.Log ("part: "+sbFloat);
		start++;
		if (valid) {
			return ParseFloat (sbFloat);
		} else {
			return 0.0f;
		}
	}


	private int GetInt(StringBuilder sb, ref int start, ref StringBuilder sbInt)
	{
		sbInt.Remove(0, sbInt.Length);
		while (start < sb.Length &&
			(char.IsDigit(sb[start])))
		{
			sbInt.Append(sb[start]);
			start++;
		}
		start++;

		return IntParseFast(sbInt);
	}


	private static float[] GenerateLookupTable()
	{
		var result = new float[(-MIN_POW_10 + MAX_POW_10) * 10];
		for (int i = 0; i < result.Length; i++)
			result[i] = (float)((i / NUM_POWS_10) *
				Mathf.Pow(10, i % NUM_POWS_10 + MIN_POW_10));
		return result;
	}

	private float ParseFloat(StringBuilder value)
	{
		float result = 0;
		bool negate = false;
		int len = value.Length;
		int decimalIndex = value.Length;
		for (int i = len - 1; i >= 0; i--)
			if (value[i] == '.')
			{ decimalIndex = i; break; }
		int offset = -MIN_POW_10 + decimalIndex;
		for (int i = 0; i < decimalIndex; i++)
			if (i != decimalIndex && value[i] != '-')
				result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i - 1];
			else if (value[i] == '-')
				negate = true;
		for (int i = decimalIndex + 1; i < len; i++)
			if (i != decimalIndex)
				result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i];
		if (negate)
			result = -result;
		return result;
	}

	private int IntParseFast(StringBuilder value)
	{
		// An optimized int parse method.
		int result = 0;
		for (int i = 0; i < value.Length; i++)
		{
			result = 10 * result + (value[i] - 48);
		}
		return result;
	}
}

public sealed class Vector3Int
{
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public Vector3Int(){}

	public Vector3Int(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}