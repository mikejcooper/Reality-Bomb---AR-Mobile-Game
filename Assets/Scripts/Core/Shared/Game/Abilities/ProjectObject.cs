using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectObject : MonoBehaviour {

	public delegate void OnPositionsSet();
	public delegate void OnFinshedStartMovement();
	public event OnPositionsSet OnPositionsSetEvent = delegate {};
	public event OnFinshedStartMovement OnFinshedStartMovementEvent = delegate {};


	private List<Vector3> _positions; 
	private bool _finshedStartMovement = false;
	private float _yOffset = 10.0f;
	private float _speed = 10.0f;
	private float _positionScale= 1.03f;

	void Start()
	{  
		StartCoroutine(StartObjectMovement());
	}
		
	public void Launch (Transform Source, Vector3 Target, float firingAngle = 45.0f) {
		StartCoroutine(Launch_Enum (Source, Target, firingAngle));
	}

	IEnumerator Launch_Enum(Transform Source, Vector3 Target, float firingAngle = 45.0f) {
		//Decouple with Projection Obj
		Source.SetParent (transform);

		Vector3 cannonTarget = Target;

		// This stops the cannon from rotating about the y axis
		cannonTarget.y = transform.position.y;

		gameObject.transform.LookAt (cannonTarget);


		yield return new WaitForSeconds (1);

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

		if (s.x > t.x)
			angleBetweenObjects = -angleBetweenObjects;

		Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

		//Decouple from Projection Obj
		Source.parent = GameObject.Find("Marker scene").transform;
		// Fire!
		Source.GetComponent<Rigidbody>().velocity = finalVelocity;

	}

	IEnumerator StartObjectMovement(){
		// Hack to avoid outer boundary of mini game when projecting first few powerupd
		transform.position = transform.position * _positionScale * 1.2f;

		// Move Object up by _yOffset
		Vector3 moveUp = new Vector3 (transform.position.x, transform.position.y + _yOffset, transform.position.z) * _positionScale;
		yield return StartCoroutine(MoveObject(transform.position, moveUp, 0.5f));

		OnFinshedStartMovementEvent ();
		_finshedStartMovement = true;

		yield return new WaitForSeconds (4.0f);

		if (OnPositionsSetEvent != null) {
			StartCoroutine (BeginObjectPath ());
		} else {
			OnPositionsSetEvent += () => StartCoroutine ( BeginObjectPath () );
		}
	}
		
	IEnumerator BeginObjectPath() {
		int i = 0;
		while (true) {
			i = (i == _positions.Count - 1) ? 0 : i + 1;
			Vector3 target = new Vector3 (_positions[i].x, _positions[i].y + _yOffset, _positions[i].z) * _positionScale;
			if (transform.position == target)
				continue;
			yield return StartCoroutine(MoveObject(transform.position, target, _speed));
		}
	}

	IEnumerator MoveObject(Vector3 startPos, Vector3 endPos, float _speed){

		float i = 0.0f;
		float time = Vector3.Distance(startPos, endPos) / _speed;
		float rate = 1.0f / time;
		while (i < 1.0f) {
			i += Time.deltaTime * rate;
			transform.position = Vector3.Lerp(startPos, endPos, i);
			yield return null; 
		}
	}

	public bool onFinishedStartMovement(){
		return _finshedStartMovement;
	}
		
	public void SetPositions(List<Vector3> positions){
		_positions = positions;
		OnPositionsSetEvent ();
	}

	public void SetHeight(float yOffset){
		_yOffset = yOffset;
	}

	public void SetSpeed(float speed){
		_speed = speed;
	}

	public void SetPositionScale(float positionScale){
		_positionScale = positionScale;
	}

}
