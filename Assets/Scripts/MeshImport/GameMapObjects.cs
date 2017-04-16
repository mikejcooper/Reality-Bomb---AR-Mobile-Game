using UnityEngine;
using System;

public class GameMapObjects
{

	public GameObject ground;
	public GameObject boundary;
//	public Vector3[] boundingBox;


	public GameMapObjects(GameObject meshGround, GameObject meshBoundary)//, Vector3[] boundingBox)
	{
		ground = meshGround;
		boundary = meshBoundary;
//		this.boundingBox = boundingBox;
	}
}


