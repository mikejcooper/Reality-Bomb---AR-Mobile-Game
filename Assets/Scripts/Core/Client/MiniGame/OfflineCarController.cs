using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class OfflineCarController : MonoBehaviour
{
	// temporary hack to allow tank prefab to be spawned and played without a network system
	public bool IsPlayingSolo = false;

	// Used to identify which tank belongs to which player.  This is set by this tank's manager.
	public int PlayerNumber = 1;
	// How fast the tank moves forward and back.
	public float Speed = 3f;
	// How fast the tank turns in degrees per second.
	public float TurnSpeed = 180f;
	public float MaxLifetime = 15.0f;
	public float PowerUpEndTime;
	public bool PowerUpActive;

	private UIJoystick _joystick;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Vector3 _direction;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);
	private int _disabled = 0;
	private float _lifetime;
	private Text _lifetimeText;


	private void Awake ()
	{
		_rigidbody = GetComponent<Rigidbody> ();
		_lifetime = MaxLifetime;

		_lifetimeText.text = "Time Left: " + string.Format("{0:N2}", _lifetime);

		PowerUpActive = false;
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


	private void Start ()
	{
		if (GameObject.Find("JoystickBack") != null) {
			_joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
		}
		if (GameObject.Find ("TimeLeftText") != null) {
			_lifetimeText = GameObject.Find ("TimeLeftText").gameObject.GetComponent<Text> ();
		}
		// The axes names are based on player number.

	}


	private void Update ()
	{

		if (PowerUpActive  && Time.time > PowerUpEndTime) {
			PowerUpActive = false;
			Speed = 30.0f;
			print ("PowerUp Deactivated");
		}
	}

	private void FixedUpdate ()
	{
		// we should find a proper way to spawn tanks so we don't need to rely
		// on isPlayingSolo
		if (_disabled > 0)
			_disabled--;

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

		Vector3 movement = transform.forward * joystickVector.magnitude * Speed * Time.deltaTime;

		_rigidbody.position += movement;

	}
}

