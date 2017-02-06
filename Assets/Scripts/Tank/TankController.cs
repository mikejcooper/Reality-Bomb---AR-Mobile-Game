﻿using UnityEngine;
using UnityEngine.Networking;


public class TankController : NetworkBehaviour
{
	public bool isPlayingSolo = false; // temporary hack to allow tank prefab to be spawned and played without a network system

    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 3f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
   
    private UIJoystick m_Joystick;
    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private Vector3 m_Direction;

	private Quaternion lookAngle = 	Quaternion.Euler(Vector3.forward);

    [SyncVar]
    public bool hasBomb = false;
    private int disabled = 0;
    private float transferTime = Time.time;


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
        // The axes names are based on player number.
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
		m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        //Set the colour
        if (hasBomb)
        {
            ChangeColour(Color.red);
        }
        else
        {
            ChangeColour(Color.blue);
        }
    }


    private void Update ()
    {


    }

    private void ChangeColour(Color colour)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                    m.color = colour;
            }
        }
    }

    private void FixedUpdate ()
    {
        // we should find a proper way to spawn tanks so we don't need to rely
        // on isPlayingSolo
        if (disabled > 0)
            disabled--;

        if (!isLocalPlayer && !isPlayingSolo)
		{ 
			return;
		}

		Vector3 joystickVector = new Vector3 (m_Joystick.Horizontal (), m_Joystick.Vertical (), 0);
		GameObject ARCamera = GameObject.Find ("ARCamera");
		Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

		if (m_Joystick.IsDragging ()) {
			lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
			// think about combining z and y so that it moves away when close to 0 degrees
			float combined = lookAngle.eulerAngles.y;
			lookAngle.eulerAngles = new Vector3(0, combined, 0);
		}

		m_Rigidbody.rotation = lookAngle;

		Vector3 movement = transform.forward * joystickVector.magnitude * m_Speed * Time.deltaTime;

		m_Rigidbody.position += movement;

    }

    bool TransferBomb()
    {
        if (hasBomb && Time.time > transferTime)
        {
            //transferTime = Time.time + 1.0f;
            hasBomb = false;
            ChangeColour(Color.blue);
            return true;
        }
        return false;
    }

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "TankTag") {
            if (col.gameObject.GetComponent<TankController>().TransferBomb())
            {
                hasBomb = true;
                ChangeColour(Color.red);
                transferTime = Time.time + 1.0f;
            }            
        }
	}

}
