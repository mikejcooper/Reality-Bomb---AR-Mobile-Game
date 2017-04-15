using Abilities;
using Powerups;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class OfflineCarController : MonoBehaviour
{
	public CarProperties CarProperties;

	private Joystick _joystick;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);


	private void OnEnable ()
	{
		// When the tank is turned on, make sure it's not kinematic.
		_rigidbody.isKinematic = false;
	}


	private void OnDisable ()
	{
		// When the tank is turned off, set it to kinematic so it stops moving.
		_rigidbody.isKinematic = true;
	}


	private void Awake ()
	{
		_joystick = GameObject.FindObjectOfType<Joystick> ();

		_rigidbody = GetComponent<Rigidbody> ();

	}


	private void Update ()
	{
		transform.rotation= Quaternion.Lerp (transform.rotation, _lookAngle , CarProperties.TurnRate * Time.deltaTime);
	}

	private void FixedUpdate ()
	{

		Vector3 joystickVector = new Vector3 (_joystick.NormalisedVector.x, _joystick.NormalisedVector.y, 0);
		GameObject ARCamera = GameObject.Find ("ARCamera");
		Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

		if (_joystick.Active) {
			_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = _lookAngle.eulerAngles.y;
			_lookAngle.eulerAngles = new Vector3(0, combined, 0);
		}

		if (_joystick.Active) {
			_rigidbody.AddForce (transform.forward * joystickVector.magnitude * CarProperties.Acceleration);
		}

		if(_rigidbody.velocity.magnitude > CarProperties.MaxSpeed) {
			_rigidbody.velocity = _rigidbody.velocity.normalized * CarProperties.MaxSpeed;
		}
	}

	void OnCollisionEnter(Collision col) {

		if (AbilityRouter.IsAbilityObject (col.gameObject)) {
			SandBoxPowerUpManager spm = GameObject.FindObjectOfType<SandBoxPowerUpManager>();
			AbilityRouter.RouteTag (AbilityRouter.GetAbilityTag(col.gameObject), CarProperties, gameObject, spm, true);
			Destroy(col.gameObject);
		} else {
			// If two players collide, calculate the angle of collision, reverse the direction and add a force in that direction
			var bounceForce = 350;
			Vector3 direction = col.contacts [0].point - transform.position;
			direction = -direction.normalized;
			direction.y = 0;
			GetComponent<Rigidbody> ().AddForce (direction * bounceForce);
		}
    }
}

