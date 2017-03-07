using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarController : NetworkBehaviour
{
	// temporary hack to allow tank prefab to be spawned and played without a network system
	public bool IsPlayingSolo = false; 

	public CarProperties CarProperties;

	// How fast the tank turns in degrees per second.
	public float TurnSpeed = 180f;
	public float MaxLifetime = 60.0f;
	public bool HasBomb = false;
	public bool Alive = true;
	public float FallDistanceBeforeRespawn = -150f;
	public int DisabledControlDurationSeconds = 2;
	// This is a server-only field that doesn't get updated on clients. I'll move this
	// at some point to a better place.
	public bool HasLoadedGame = false;

	[SyncVar]
	public int ServerId;

	[SyncVar]
	public float Lifetime;

	private UIJoystick _joystick;
    private UIHealthBar _healthBar;
//    private Image _bombImage;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Vector3 _direction;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);
	private float _transferTime;

	private Text LifetimeText;
	private bool _initialised;
	private bool _controlsDisabled;
	private bool _preparingGame = true;

	private void Start ()
	{
		if (GameObject.FindObjectOfType<GameManager> () != null) {
			init ();
		}

	}

	public void init () {

		if (!_initialised) {
			if (GameObject.Find ("JoystickBack") != null) {
				_joystick = GameObject.Find ("JoystickBack").gameObject.GetComponent<UIJoystick> ();
			}
			Transform nameText = transform.Find ("NameTag").Find ("NameText");
			Text textComponent = nameText.gameObject.GetComponent<Text> ();
			if (isServer) {
				textComponent.text = ServerSceneManager.Instance.GetPlayerDataById (ServerId).Name;
			} else {
				textComponent.text = ClientSceneManager.Instance.GetPlayerDataById (ServerId).Name;
			}
//            if (GameObject.Find("BombImage") != null)
//            {
//                _bombImage = GameObject.Find("BombImage").gameObject.GetComponent<Image>();
//            }
            if (GameObject.Find("HealthBar") != null)
            {
                _healthBar = GameObject.Find("HealthBar").gameObject.GetComponent<UIHealthBar>();
            }
            // The axes names are based on player number.

            _rigidbody = GetComponent<Rigidbody> ();
			Lifetime = MaxLifetime;
            _healthBar.MaxValue = MaxLifetime;
            _healthBar.MinValue = 0;
			_transferTime = Time.time;
			_initialised = true;
			_controlsDisabled = false;

			// hide and freeze so we can correctly position
			if (hasAuthority) {
				_rigidbody.isKinematic = true;
				gameObject.SetActive (false);
			}

			CarProperties.PowerUpActive = false;

			if (!isServer) {
				CmdRequestColour ();
//				EnableControls (false);
			} else {
				//_preparingGame = true;
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

		bool isBomb = GameObject.FindObjectOfType<GameManager>().IsStartingBomb(connectionToClient.connectionId) ;
		AllDevicesSetBomb (isBomb);
	}

	public void AllDevicesSetBomb(bool isBomb) {
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
			GameObject.FindObjectOfType<GameManager> ().BombObject.transform.parent = transform;
			GameObject.FindObjectOfType<GameManager> ().BombObject.transform.localScale = 0.01f * Vector3.one;
			GameObject.FindObjectOfType<GameManager> ().BombObject.transform.localPosition = new Vector3 (0, 2.5f, 0);
			GameObject.FindObjectOfType<GameManager> ().BombObject.SetActive (true);
//			ChangeColour (Color.red);
            
		} else {
//			ChangeColour (Color.blue);
		}
//        if (isLocalPlayer || IsPlayingSolo)
//        {
//            _bombImage.enabled = HasBomb;
//        }
	}


	private void Update ()
	{
		if (!_initialised)
			return;
				
		if ((isLocalPlayer || IsPlayingSolo)) {
            //_healthBar.Value = Lifetime;
            if (!_preparingGame)
                _healthBar.UpdateCountdown(Lifetime, HasBomb);

            if (CarProperties.PowerUpActive && Time.time > CarProperties.PowerUpEndTime) {
				CarProperties.PowerUpActive = false;
				CarProperties.Speed = 30.0f;
				print ("PowerUp Deactivated");
			}
			EnsureCarIsOnMap ();
		} else if (isServer) {	
			// let the server authoratively update vital stats
			if ((HasBomb && Lifetime > 0.0f) && !_preparingGame) {
				Debug.Log("PreparingGame: " + _preparingGame);
				Lifetime -= Time.deltaTime;
			}
			if (Lifetime < 0.0f) {
				Kill ();
			}
		}
	}
		
	[Server]
	private void Kill () {
		DebugConsole.Log ("player has run out of time");
		Lifetime = 0.0f;
		Alive = false;
		GameObject.FindObjectOfType<GameManager>().KillPlayer (this);
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

		if (!_controlsDisabled) {
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

	public bool IsTransferTimeExpired()
	{
		return Time.time > _transferTime;
	}

	public void UpdateTransferTime(float inc){
		_transferTime = Time.time + inc;
	}

	[ServerCallback]
	void OnCollisionEnter(Collision col)
	{
		GameObject.FindObjectOfType<GameManager> ().CollisionEvent (this, col);
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

	[Server]
	public void ServerGameStarting(){
        _preparingGame = false;
        DebugConsole.Log("Server: " + _preparingGame);
	}

	[ClientRpc]
	public void RpcPlayerGameStarting(){
        _preparingGame = false;
		DebugConsole.Log("Player: " + _preparingGame);
        _healthBar.UpdateCountdown(Lifetime, HasBomb);
        EnableControls (true);
	}

	public void EnsureCarIsOnMap(){
		if(_rigidbody.position.y <= FallDistanceBeforeRespawn){
			Reposition (GameObject.FindObjectOfType<GameManager> ().WorldMesh);
			DisableControls (DisabledControlDurationSeconds);
		}
	}

	public void KillAllControls(){
		RpcKillAllControls ();
	}

	[ClientRpc]
	public void RpcKillAllControls(){

		if (hasAuthority) {
			Texture tex = Resources.Load("DiedText.png") as Texture;
			GUI.DrawTexture(new Rect(10, 10, 60, 60), tex, ScaleMode.ScaleToFit, true, 10.0F);
		}


		_controlsDisabled = true;
	}

	[Server]
	public void ServerKillAllControls(){
		_controlsDisabled = true;
	}


	public void DisableControls(int seconds){
		ToggleControls();
		Invoke("ToggleControls", seconds);
	}



	[ClientRpc]
	public void RpcEnableALLControls(bool b) {
		DebugConsole.Log ("Enabled");
		if (b) {
			_controlsDisabled = false;
		} else {
			_controlsDisabled = true;
		}
	}

	public void ToggleControls(){
		_controlsDisabled = !_controlsDisabled;
	}

	public void EnableControls(bool b){
		if (b) {
			_controlsDisabled = false;
		} else {
			_controlsDisabled = true;
		}
	}
}