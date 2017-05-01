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
    private SandBoxPowerUpManager _spm;

	void Start () {
		CarProperties.OriginalHue = 0;

        _spm = GameObject.FindObjectOfType<SandBoxPowerUpManager>();
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
		transform.rotation = Quaternion.RotateTowards (transform.rotation, _lookAngle, CarProperties.SafeTurnRate * Time.deltaTime);
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

			// how close are we to facing the direction the user wants?
			float dirFactor = Mathf.Max(0,Vector3.Dot(transform.forward, _lookAngle * Vector3.forward));

			_rigidbody.AddForce (transform.forward * dirFactor * joystickVector.magnitude * CarProperties.SafeAccel);
		}

		transform.localScale = Vector3.one * CarProperties.SafeScale;

		if(_rigidbody.velocity.magnitude > CarProperties.SafeSpeedLimit) {
			_rigidbody.velocity = _rigidbody.velocity.normalized * CarProperties.SafeSpeedLimit;
		}
	}

	void OnCollisionEnter(Collision col) {

		if (col.gameObject.CompareTag("PowerUp")) {
			OnPowerupCollision (col.gameObject);
		}
    }

	private void OnPowerupCollision (GameObject obj) {
		string type = _spm.GetPowerupType (obj, false);
		AbilityRouter.RouteTag (type, CarProperties, gameObject, _spm, true, true);

        _spm.PowerUpPool.UnSpawnObject(obj);
    }

    private void OnTriggerEnter(Collider other)
    {
		if (other.transform.parent.gameObject.CompareTag("PowerUp")) {
			OnPowerupCollision (other.transform.parent.gameObject);
        }
    }		
}

