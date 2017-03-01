using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class OfflineCarController : MonoBehaviour
{
	public CarProperties CarProperties;

	private UIJoystick _joystick;

	private GameObject _ARCamera;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);

	// Reference used to move the tank.
	private CharacterController _controller;

	private void OnEnable ()
	{
		// When the tank is turned on, make sure it's not kinematic.
		//_rigidbody.isKinematic = false;
	}


	private void OnDisable ()
	{
		// When the tank is turned off, set it to kinematic so it stops moving.
		//_rigidbody.isKinematic = true;
	}


	private void Awake ()
	{
		if (GameObject.Find("JoystickBack") != null) {
			_joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
		}

		if (GameObject.Find ("ARCamera") != null) {
			_ARCamera = GameObject.Find ("ARCamera");
		}

		//_rigidbody = GetComponent<Rigidbody> ();

		_controller = GetComponent<CharacterController> ();

		CarProperties.PowerUpActive = false;

	}


	private void Update ()
	{
		
	}

	private void FixedUpdate ()
	{

		Vector3 joystickVector = new Vector3 (_joystick.Horizontal (), _joystick.Vertical (), 0);
		Vector3 rotatedVector = _ARCamera.transform.rotation * joystickVector;

		if (_joystick.IsDragging ()) 
		{
			_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = _lookAngle.eulerAngles.y;
			_lookAngle.eulerAngles = new Vector3 (0, combined, 0);

			Vector3 joystickVector2 = new Vector3(_joystick.Horizontal(), 0.0f, _joystick.Vertical());

			if (CarProperties.CurrentVelocity.magnitude < CarProperties.MaxSpeed * joystickVector2.magnitude)
				CarProperties.CurrentVelocity += Time.deltaTime * CarProperties.Acceleration * joystickVector2;
			else
				CarProperties.CurrentVelocity -= Time.deltaTime * CarProperties.Acceleration * CarProperties.CurrentVelocity.normalized;
		} 
		else 
		{
			if (CarProperties.CurrentVelocity.magnitude > 2.0f)
				CarProperties.CurrentVelocity -= Time.deltaTime * CarProperties.Acceleration * CarProperties.CurrentVelocity.normalized;
			else
				CarProperties.CurrentVelocity = Vector3.zero;

		}
			
		transform.rotation = _lookAngle;

		_controller.SimpleMove(CarProperties.CurrentVelocity);


	}
}

