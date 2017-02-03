using UnityEngine;
using UnityEngine.Networking;


public class TankMovement : NetworkBehaviour
{
	public bool isPlayingSolo = false; // temporary hack to allow tank prefab to be spawned and played without a network system

    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 3f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
    public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
    public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
	public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

    private UIJoystick m_Joystick;
    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
//    private float m_MovementInputValue;         // The current value of the movement input.
//    private float m_TurnInputValue;             // The current value of the turn input.
    private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.


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
//        m_MovementInputValue = 0f;
//        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }


    private void Start ()
    {
        m_Joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
        // The axes names are based on player number.
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        // Store the original pitch of the audio source.
        m_OriginalPitch = m_MovementAudio.pitch;
    }


    private void Update ()
    {
		// we should find a proper way to spawn tanks so we don't need to rely
		// on isPlayingSolo
        if (!isLocalPlayer && !isPlayingSolo)
        { 
            return;
        }
        // Store value of both input axes from joystick


//        EngineAudio ();
    }

//
//    private void EngineAudio ()
//    {
//        // If there is no input (the tank is stationary)...
//        if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
//        {
//            // ... and if the audio source is currently playing the driving clip...
//            if (m_MovementAudio.clip == m_EngineDriving)
//            {
//                // ... change the clip to idling and play it.
//                m_MovementAudio.clip = m_EngineIdling;
//                m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
//                m_MovementAudio.Play ();
//            }
//        }
//        else
//        {
//            // Otherwise if the tank is moving and if the idling clip is currently playing...
//            if (m_MovementAudio.clip == m_EngineIdling)
//            {
//                // ... change the clip to driving and play.
//                m_MovementAudio.clip = m_EngineDriving;
//                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
//                m_MovementAudio.Play();
//            }
//        }
//    }


    private void FixedUpdate ()
    {
//		m_MovementInputValue  = m_Joystick.Vertical();
//		m_TurnInputValue = m_Joystick.Horizontal ();

		// face the direction of the joystick

		var angle = 90 - Mathf.Atan2(m_Joystick.Vertical(), m_Joystick.Horizontal()) * Mathf.Rad2Deg;
//		Debug.Log(string.Format ("({0} , {1}) => {2}", m_Joystick.Horizontal(), m_Joystick.Vertical(), angle));

		Vector3 joystickVector = new Vector3 (m_Joystick.Horizontal (), m_Joystick.Vertical (), 0);


		GameObject ARCamera = GameObject.Find ("ARCamera");

//		Debug.Log (ARCamera.transform.rotation.eulerAngles);

		Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

		// think about combining z and y so that it moves away when close to 0 degrees

		Quaternion lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
//		Debug.Log (string.Format ("{0} => {1} => {2}", joystickVector, rotatedVector, lookAngle.eulerAngles));

//		m_Rigidbody.rotation = ARCamera.transform.rotation * Quaternion.AngleAxis (angle, new Vector3 (0, 1, 0)); // probably should use tank normal rather than y axis

//		m_Rigidbody.rotation = Quaternion.FromToRotation (new Vector3 (1, 1, 1), new Vector3 (1, 1, 1));
		lookAngle.eulerAngles = new Vector3(0, lookAngle.eulerAngles.y, 0);
		m_Rigidbody.rotation = lookAngle;




		Vector3 movement = transform.forward * m_Speed * Time.deltaTime;

		Debug.Log (string.Format ("{0} => {1} => {2}", lookAngle.eulerAngles, movement, movement*joystickVector.magnitude));
		movement *= joystickVector.magnitude;

		m_Rigidbody.position += movement;

//		float Joystick_angle = Mathf.Atan2(m_TurnInputValue, m_MovementInputValue) * Mathf.Rad2Deg; 
//		float Camera_angle = 0;//Mathf.Atan2(Camera.main.transform.rotation.x, Camera.main.transform.rotation.y) * Mathf.Rad2Deg; 
//		// Maths not quite right here?? "Seems to work" when 2*Camera_angle
//		float Direction_angle = Joystick_angle + 2*Camera_angle;
//		if (Joystick_angle == 0) {
//			float Tank_angle = m_Rigidbody.transform.rotation.eulerAngles.y;
//			Direction_angle = Tank_angle;
//		}
//
//		// Apply this rotation to the rigidbody's rotation.
//		m_Rigidbody.rotation = Quaternion.Euler(new Vector3(0f, Direction_angle, 0f));
//		m_Rigidbody.MoveRotation (m_Rigidbody.rotation);
//	
//		// Get direction from the absolute joysitck input values. 
//		float direction = Mathf.Min(1,Mathf.Abs(m_MovementInputValue) + Mathf.Abs(m_TurnInputValue));
		// Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
//		Vector3 movement = transform.forward * direction * m_Speed * Time.deltaTime;
		// Apply this movement to the rigidbody's position.
//		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


}
