using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarController : NetworkBehaviour
{
	public bool isPlayingSolo = false; // temporary hack to allow tank prefab to be spawned and played without a network system

    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 3f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
    public float m_MaxLifetime = 15.0f;
   
    private UIJoystick m_Joystick;
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private Vector3 m_Direction;

	private Quaternion lookAngle = 	Quaternion.Euler(Vector3.forward);

    [SyncVar]
    public bool hasBomb = false;
    public bool alive = true;
    private float m_transferTime;
    [SyncVar]
    private float m_Lifetime;
    private Text m_LifetimeText;

	public float powerUpEndTime;
	public bool powerUpActive;


    private void Awake ()
    {
        m_Rigidbody = GetComponent<Rigidbody> ();
        m_Lifetime = m_MaxLifetime;
        m_transferTime = Time.time;
        
		powerUpActive = false;
    }


    private void OnEnable ()
    {
        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;
    }


    private void OnDisable ()
    {
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }


    private void Start ()
    {
		if (GameObject.Find("JoystickBack") != null) {
			m_Joystick = GameObject.Find("JoystickBack").gameObject.GetComponent<UIJoystick>();
		}
		if (GameObject.Find ("TimeLeftText") != null) {
			m_LifetimeText = GameObject.Find ("TimeLeftText").gameObject.GetComponent<Text> ();
		}
        // The axes names are based on player number.

        //Set the colour
        if (hasBomb)
        {
            ChangeColour(Color.red);
        }else{
            ChangeColour(Color.blue);
        }
    }


    private void Update ()
    {
		if (isLocalPlayer  || isPlayingSolo) {
            m_LifetimeText.text = "Time Left: " + string.Format("{0:N2}", m_Lifetime);

            if (powerUpActive  && Time.time > powerUpEndTime) {
				powerUpActive = false;
				m_Speed = 30.0f;
				print ("PowerUp Deactivated");
			}
		}

        //Everything after this is only executed on the server
        if (!isServer){
            return;
        }
        if (hasBomb && m_Lifetime > 0.0f){
            m_Lifetime -= Time.deltaTime;
        }
        if (m_Lifetime < 0.0f){
            m_Lifetime = 0.0f;
            alive = false;
        }
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
        if (hasBomb && Time.time > m_transferTime)
        {
            hasBomb = false;
            ChangeColour(Color.blue);
            return true;
        }
        return false;
    }

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "TankTag") {
            if (col.gameObject.GetComponent<CarController>().TransferBomb())
            {
                hasBomb = true;
                ChangeColour(Color.red);
                m_transferTime = Time.time + 1.0f;
            }            
        }
	}

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isServer)
        {
            PTBGameManager.AddCar(gameObject);
        }
    }

    public void Reposition()
    {
        if (!hasAuthority)
            return;

        
        Rigidbody rbd = GetComponent<Rigidbody>();

        Debug.Log("Repositioning car");

        Bounds bounds = DataTransferManager.s_WorldMesh.transform.GetComponent<MeshRenderer>().bounds;
        Vector3 center = bounds.center;
       
        //Set velocities to zero
        rbd.velocity = Vector3.zero;
        rbd.angularVelocity = Vector3.zero;

        for (int i = 0; i < 30; i++)
        {
            Debug.Log("Trying random position " + i);
            float x = UnityEngine.Random.Range(center.x - (bounds.size.x / 2), center.x + (bounds.size.x / 2));
            float z = UnityEngine.Random.Range(center.x - (bounds.size.z / 2), center.z + (bounds.size.z / 2));

            Vector3 position = new Vector3(x, center.y + bounds.size.y, z);
            RaycastHit hit;

            if (Physics.Raycast(position, Vector3.down, out hit, bounds.size.y * 2))
            {
                position.y = hit.point.y;
                rbd.position = position;
                break;
            }
        }
    }
}
