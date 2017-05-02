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
	private float _yOffset = 4.0f;
	private float _speed = 10.0f;

	private Vector3 _cannonTarget;
	private Vector3 _direction;
	private Quaternion _lookRotation;
	private float _rotationSpeed = 2.0f;

	private bool _rotateCannon;
	public GameObject CannonObj;

	void Start()
	{ 	
		_rotateCannon = false;
		
	}

	void Update()
	{
		if (_rotateCannon) {
			//find the vector pointing from our position to the target
			_direction = (_cannonTarget - CannonObj.transform.position).normalized;
			_direction.y = 0;
			//create the rotation we need to be in to look at the target
			_lookRotation = Quaternion.LookRotation (_direction);

			//rotate us over time according to speed until we are in the required rotation
			CannonObj.transform.rotation = Quaternion.Slerp (CannonObj.transform.rotation, _lookRotation, Time.deltaTime * _rotationSpeed);
		}
	}
		
	public void Launch (Transform Source, Vector3 Target, float firingAngle = 45.0f) {
		StartCoroutine(Launch_Enum (Source, Target, firingAngle));
	}

	IEnumerator Launch_Enum(Transform Source, Vector3 Target, float firingAngle = 45.0f) {
		Source.GetComponent<Rigidbody> ().useGravity = false;
		Source.GetComponent<BoxCollider> ().enabled = false;

		//Decouple with Projection Obj
		Source.SetParent (transform);

		_cannonTarget = Target;
		_cannonTarget.y = 4;

		_rotateCannon = true;

		yield return new WaitForSeconds (0.1f);

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
		// Reenable components
		Source.GetComponent<Rigidbody> ().useGravity = true;
		Source.GetComponent<BoxCollider> ().enabled = true;
		// Fire!
		Source.GetComponent<Rigidbody>().velocity = finalVelocity;

	}

	IEnumerator StartObjectMovement(){
		// Hack to avoid outer boundary of mini game when projecting first few powerupd
		transform.position = transform.position;

		// Move Object up by _yOffset
		Vector3 moveUp = new Vector3 (transform.position.x, transform.position.y + _yOffset, transform.position.z);
		yield return StartCoroutine(MoveObject(transform.position, moveUp, 1f));

		OnFinshedStartMovementEvent ();
		_finshedStartMovement = true;

		yield return new WaitForSeconds (2.0f);

		//if (OnPositionsSetEvent != null) {
		StartCoroutine (BeginObjectPath (_positions));
		//} else {
		//	OnPositionsSetEvent += () => StartCoroutine ( BeginObjectPath (_positions) );
		//}
	}
		
	IEnumerator BeginObjectPath(List<Vector3> positions) {
		while (true) {
			foreach(Vector3 p in positions) {
				Vector3 target = new Vector3 (p.x, p.y + _yOffset, p.z);
				if (ArePositionsClose (target, transform.position, 3.0f))
					continue;
				yield return StartCoroutine(MoveObject(transform.position, target, _speed));
			}
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

	private bool ArePositionsClose(Vector3 p1, Vector3 p2, float threshold){
		float distance = Vector3.Distance(p1, p2);
		if (distance < threshold)
			return true;
		return false;
	}

	public bool onFinishedStartMovement(){
		return _finshedStartMovement;
	}
		
	public void SetPositions(List<Vector3> positions){
		_positions = positions;
        //OnPositionsSetEvent ();
        StartCoroutine(StartObjectMovement());
    }

	public void SetHeight(float yOffset){
		_yOffset = yOffset;
	}

	public void SetSpeed(float speed){
		_speed = speed;
	}
}
