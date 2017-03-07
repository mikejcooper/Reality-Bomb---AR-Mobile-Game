using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class OfflineCarController : MonoBehaviour
{
	public CarProperties CarProperties;

	private UIJoystick _joystick;
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
		if (GameObject.Find("JoystickBack") != null) {
			_joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
		}


		_rigidbody = GetComponent<Rigidbody> ();

		CarProperties.PowerUpActive = false;

	}


	private void Update ()
	{
		
	}

	private void FixedUpdate ()
	{

		Vector3 joystickVector = new Vector3 (_joystick.Horizontal (), _joystick.Vertical (), 0);
		GameObject ARCamera = GameObject.Find ("ARCamera");
		Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

		if (_joystick.IsDragging ()) {
			_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = _lookAngle.eulerAngles.y;
			_lookAngle.eulerAngles = new Vector3(0, combined, 0);
		}

		_rigidbody.rotation = _lookAngle;


		if (_joystick.IsDragging ()) {
			
			_rigidbody.velocity = CarProperties.Speed * transform.forward * joystickVector.magnitude;

		}


	}
}

