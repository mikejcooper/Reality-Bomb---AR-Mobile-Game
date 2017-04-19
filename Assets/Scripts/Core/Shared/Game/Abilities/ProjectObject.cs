using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectObject : MonoBehaviour {

//	public Transform Target;
//	public float firingAngle = 45.0f;
//	public float gravity = 9.8f;
//	public Transform Source;    

	private Transform myTransform;

	void Awake()
	{
		myTransform = transform;      
	}

	void Start()
	{          
//		Launch();
	}
		

	public void Launch (Transform Source, Vector3 Target, float firingAngle = 45.0f) {
		// Move source to start position + some offset. 
		Source.position = myTransform.position + new Vector3(0, 0.0f, 0);

		var rigid = Source.GetComponent<Rigidbody>();

		Vector3 t = Target;
		Vector3 s = Source.position;

		float gravity = Physics.gravity.magnitude;
		// Selected angle in radians
		float angle = firingAngle * Mathf.Deg2Rad;

		// Positions of this object and the target on the same plane
		Vector3 planarTarget = new Vector3(t.x, 0, t.z);
		Vector3 planarPostion = new Vector3(s.x, 0, s.z);

		// Planar distance between objects
		float distance = Vector3.Distance(planarTarget, planarPostion);
		// Distance along the y axis between objects
		float yOffset = s.y - t.y;

		float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

		Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

		// Rotate our velocity to match the direction between the two objects
		float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
		Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

		// Fire!
		rigid.velocity = finalVelocity;
	}
}
