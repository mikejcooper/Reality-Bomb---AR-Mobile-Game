﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarController : NetworkBehaviour
{
	// temporary hack to allow tank prefab to be spawned and played without a network system
	public bool IsPlayingSolo = false; 

	public CarProperties CarProperties;

	// How fast the tank turns in degrees per second.
	public float TurnSpeed = 180f;
	public float MaxLifetime = 15.0f;
	public bool HasBomb = false;
	public bool Alive = true;


	private UIJoystick _joystick;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Vector3 _direction;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);
	private float _transferTime;
	[SyncVar]
	private float _lifetime;
	private Text _lifetimeText;
	private bool _initialised;




	private void Start ()
	{
		if (GameObject.FindObjectOfType<GameManager> () != null) {
			init ();
		}

	}

	public void init () {

		if (!_initialised) {
			DebugConsole.Log ("CarController start");
			if (GameObject.Find ("JoystickBack") != null) {
				_joystick = GameObject.Find ("JoystickBack").gameObject.GetComponent<UIJoystick> ();
			}
			if (GameObject.Find ("TimeLeftText") != null) {
				_lifetimeText = GameObject.Find ("TimeLeftText").gameObject.GetComponent<Text> ();
			}
			// The axes names are based on player number.

			_rigidbody = GetComponent<Rigidbody> ();
			_lifetime = MaxLifetime;
			_transferTime = Time.time;
			_initialised = true;

			// hide and freeze so we can correctly position
			if (hasAuthority) {
				_rigidbody.isKinematic = true;
				gameObject.SetActive (false);
			}

			CarProperties.PowerUpActive = false;

			if (!isServer) {
				CmdRequestColour ();
			} else {
				// register
				DebugConsole.Log ("GameManager.Instance.AddCar (gameObject);");
				GameObject.FindObjectOfType<GameManager> ().AddCar (gameObject);
			}

			if (GameObject.FindObjectOfType<GameManager> ().WorldMesh != null) {
				DebugConsole.Log ("available");
				Reposition (GameObject.FindObjectOfType<GameManager> ().WorldMesh);
			} else {
				DebugConsole.Log ("unavailable");
				GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent += Reposition;
			}
		}


	}

	void OnDestroy () {
		GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent -= Reposition;
	}

	[Command]
	private void CmdRequestColour () {
		DebugConsole.Log ("I am server and I choose bomb");
		// set colour based off server's game manager

		bool isBomb = connectionToClient.connectionId == GameObject.FindObjectOfType<GameManager>().BombPlayerConnectionId ;
		DebugConsole.Log (string.Format ("is {0} == {1} ? {2}", connectionToClient.connectionId, GameObject.FindObjectOfType<GameManager>().BombPlayerConnectionId, isBomb));
		AllDevicesSetBomb (isBomb);
	}

	private void AllDevicesSetBomb(bool isBomb) {
		RpcSetBomb (isBomb);
		ServerSetBomb (isBomb);
	}

	[ClientRpc]
	private void RpcSetBomb(bool isBomb) {
		processSetBombMessage (isBomb);
	}

	private void ServerSetBomb(bool isBomb) {
		processSetBombMessage (isBomb);
	}

	private void processSetBombMessage(bool isBomb) {
		DebugConsole.Log("isBomb: " + isBomb);
		HasBomb = isBomb;
		if (isBomb) {
			ChangeColour (Color.red);
		} else {
			ChangeColour (Color.blue);
		}
	}


	private void Update ()
	{
		if (!_initialised)
			return;
		
		if (isLocalPlayer || IsPlayingSolo) {
			_lifetimeText.text = "Time Left: " + string.Format ("{0:N2}", _lifetime);

			if (CarProperties.PowerUpActive && Time.time > CarProperties.PowerUpEndTime) {
				CarProperties.PowerUpActive = false;
				CarProperties.Speed = 30.0f;
				print ("PowerUp Deactivated");
			}
		} else if (isServer) {
			// let the server authoratively update vital stats
			if (HasBomb && _lifetime > 0.0f) {
				_lifetime -= Time.deltaTime;
			}
			if (_lifetime < 0.0f) {
				Kill ();
			}
		}
	}

	[Server]
	private void Kill () {
		DebugConsole.Log ("player has run out of time");
		_lifetime = 0.0f;
		Alive = false;
		GameObject.FindObjectOfType<GameManager>().AllDevicesKillPlayer (this);
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
		if (!_initialised)
			return;
		
		if (!isLocalPlayer && !IsPlayingSolo)
		{ 
			return;
		}

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

		Vector3 movement = transform.forward * joystickVector.magnitude * CarProperties.Speed * Time.deltaTime;

		_rigidbody.position += movement;

	}

	bool TransferBomb()
	{
		if (HasBomb && Time.time > _transferTime)
		{
			
			AllDevicesSetBomb(false);
			return true;
		}
		return false;
	}

	[ServerCallback]
	void OnCollisionEnter(Collision col)
	{
		
		if (col.gameObject.tag == "TankTag") {
			if (col.gameObject.GetComponent<CarController>().TransferBomb())
			{
				AllDevicesSetBomb(true);
				_transferTime = Time.time + 1.0f;
			}            
		}
	}

	public void Reposition(GameObject worldMesh)
	{

		DebugConsole.Log ("repositioning");
		if (hasAuthority) {
			DebugConsole.Log ("repositioning with authority");

			Debug.Log ("Repositioning car");

			//Set velocities to zero
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;

			Vector3 position = GameUtils.FindSpawnLocation (worldMesh);

			if (position != Vector3.zero) {
				DebugConsole.Log ("unfreezing");
				// now unfreeze and show

				gameObject.SetActive (true);
				_rigidbody.isKinematic = false;

				_rigidbody.position = position;
			}

		}



	}
}