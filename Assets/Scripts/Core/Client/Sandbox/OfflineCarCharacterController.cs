using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class OfflineCarCharacterController : MonoBehaviour
{
	public CarProperties CarProperties;

	private UIJoystick _joystick;

	private GameObject _ARCamera;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);

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
	}


	private void Update ()
	{
		
	}

	private void FixedUpdate ()
	{

		Vector3 joystickVector = new Vector3 (_joystick.Horizontal (), _joystick.Vertical (), 0);
		Vector3 rotatedVector = _ARCamera.transform.rotation * joystickVector;

		if (_joystick.IsDragging ()) {
			_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = _lookAngle.eulerAngles.y;
			_lookAngle.eulerAngles = new Vector3(0, combined, 0);
		}
			
		transform.rotation = _lookAngle;

//		_controller.SimpleMove(CarProperties.Speed * transform.forward * joystickVector.magnitude);


	}
}

