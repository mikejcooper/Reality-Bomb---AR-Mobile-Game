using UnityEngine;
using System;
using System.Collections.Generic;

public class GameMapObjects
{

	public GameObject ground;
	public GameObject boundary;
	public List<Vector3> convexhullVertices;


	public GameMapObjects(GameObject meshGround, GameObject meshBoundary,List<Vector3> convexhullVertices)//, Vector3[] boundingBox)
	{
		ground = meshGround;
		boundary = meshBoundary;
		this.convexhullVertices = convexhullVertices;
	}
}


