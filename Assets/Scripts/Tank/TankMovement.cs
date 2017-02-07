using UnityEngine;
using UnityEngine.Networking;


public class TankMovement : NetworkBehaviour
{
	public bool isPlayingSolo = false; // temporary hack to allow tank prefab to be spawned and played without a network system
    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 3f;                 // How fast the tank moves forward and back.
    private UIJoystick m_Joystick;
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.

	private Quaternion lookAngle = Quaternion.Euler(Vector3.forward);


    private void Awake ()
    {
        m_Rigidbody = GetComponent<Rigidbody> ();
		m_Joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
    }


    private void OnEnable ()
    {
        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Also reset the input values.
    }


    private void OnDisable ()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }


    private void Start ()
    {
        m_Joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
    }


    private void Update ()
    {
		// we should find a proper way to spawn tanks so we don't need to rely
		// on isPlayingSolo
        if (!isLocalPlayer && !isPlayingSolo)
        { 
            return;
        }

    }

    private void FixedUpdate ()
    {

		Vector3 joystickVector = new Vector3 (m_Joystick.Horizontal (), m_Joystick.Vertical (), 0);
		GameObject ARCamera = GameObject.Find ("ARCamera");
		Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;


		if (m_Joystick.IsDragging ()) {
			lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = lookAngle.eulerAngles.y;
			lookAngle.eulerAngles = new Vector3(0, lookAngle.eulerAngles.y, 0);
		}

//		Debug.Log (string.Format ("angle: {0}", lookAngle.eulerAngles));
		m_Rigidbody.rotation = lookAngle;

		Vector3 movement = transform.forward * joystickVector.magnitude * m_Speed * Time.deltaTime;


		if (Mathf.Abs(movement.x) > 0.0f) {
//			Debug.Log (string.Format("position: {0} movement: {1}", m_Rigidbody.position, movement));
			m_Rigidbody.position += movement;

		} else {
//			m_Rigidbody.velocity = new Vector3(0,0,0);
//			Debug.Log (string.Format("not updating position, vel: {0}", m_Rigidbody.velocity));
		}

    }


	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.name == "Tank") {
			print ("Hit Tank!");
		}
	}

}
