using Abilities;
using Powerups;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class OfflineCarController : MonoBehaviour
{
	public CarProperties CarProperties;
	public ParticleSystem SandParticles;

	private Joystick _joystick;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Quaternion _lookAngle = Quaternion.Euler (Vector3.forward);

	void Start () {
		CarProperties.OriginalHue = 0;

		Material[] materials = transform.FindChild("Car_Model").GetComponent<MeshRenderer> ().materials;

		GameUtils.SetCarMaterialColoursFromHue (materials, CarProperties.OriginalHue);
	}

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
			_rigidbody.AddForce (transform.forward * joystickVector.magnitude * CarProperties.SafeAccel);
		}

		transform.localScale = Vector3.one * CarProperties.SafeScale;

		if(_rigidbody.velocity.magnitude > CarProperties.SafeSpeedLimit) {
			_rigidbody.velocity = _rigidbody.velocity.normalized * CarProperties.SafeSpeedLimit;
		}
	}

	void OnCollisionEnter(Collision col) {

		if (col.gameObject.CompareTag("PowerUp")) {
			OnPowerupCollision (col.gameObject);
		} else {
			// If two players collide, calculate the angle of collision, reverse the direction and add a force in that direction
			var bounceForce = 350;
			Vector3 direction = col.contacts [0].point - transform.position;
			direction = -direction.normalized;
			direction.y = 0;
			GetComponent<Rigidbody> ().AddForce (direction * bounceForce);
		}
    }

	private void OnPowerupCollision (GameObject obj) {
		SandBoxPowerUpManager spm = GameObject.FindObjectOfType<SandBoxPowerUpManager> ();
		string type = spm.GetPowerupType (obj, false);
		AbilityRouter.RouteTag (type, CarProperties, gameObject, spm, true, true);
		Destroy(obj);
	}

    private void OnTriggerEnter(Collider other)
    {
		if (other.transform.parent.gameObject.CompareTag("PowerUp")) {
			OnPowerupCollision (other.transform.parent.gameObject);
        }
    }		
}

