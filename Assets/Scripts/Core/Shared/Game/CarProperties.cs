using System;
using UnityEngine;

public class CarProperties : MonoBehaviour
{
	// How fast the tank moves forward and back.
	public Vector3 CurrentVelocity = Vector3.zero;
	public float MaxSpeed = 100.0f;
	public float Acceleration = 400.0f;

	public float PowerUpEndTime;
	public bool PowerUpActive;

}


