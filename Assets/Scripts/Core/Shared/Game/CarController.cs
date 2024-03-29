﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Abilities;
using Powerups;

public class CarController : NetworkBehaviour
{
	public delegate void OnSetBomb (bool bomb);
	public event OnSetBomb OnSetBombEvent;

	// temporary hack to allow tank prefab to be spawned and played without a network system
	public bool IsPlayingSolo = false; 

	public CarProperties CarProperties;

	// How fast the tank turns in degrees per second.
	[SyncVar(hook="OnMaxLifetimeUpdate")]
	public float MaxLifetime = 60.0f;
	public bool HasBomb = false;
	public bool Alive = true;
	public float BombSpeedBoostFactor = 1.1f;
	public int DisabledControlDurationSeconds = 2;
	// This is a server-only field that doesn't get updated on clients. I'll move this
	// at some point to a better place.
	public GameObject ExplosionAnimation;

	private AudioSource ExplosionSound;
	private AudioSource PowerUpSound;
	private AudioSource BombAlertSound;
	private AudioSource BumpSound;
	private AudioSource PositiveSound;



	[SyncVar]
	public int ServerId;

	[SyncVar]
	public float Lifetime;



	private Joystick _joystick;
    private UIHealthBar _healthBar;
    private GameManager _gameManager;
    //    private Image _bombImage;
    // Reference used to move the tank.
    private Rigidbody _rigidbody;
    private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);
	private float _transferTime;
	private float _fallDistanceBeforeRespawn = -3.0f; //default value in case it does not get updated


	private bool _initialised;
	private bool _controlsDisabled;

	private void Start ()
	{
		if (GameObject.FindObjectOfType<GameManager> () != null) {
			init ();
		}

	}

	public NetworkCompat.NetworkLobbyPlayer LobbyPlayer () {
		
		foreach (var lobbyPlayer in GameObject.FindObjectsOfType<NetworkCompat.NetworkLobbyPlayer> ()) {
			if (lobbyPlayer.serverId == ServerId) {
				return lobbyPlayer;
			}
		}

		return null;
	}

	void OnMaxLifetimeUpdate (float maxLifetime) {
		MaxLifetime = maxLifetime;
		if (GameObject.Find ("HealthBar") != null) {
			_healthBar = GameObject.Find ("HealthBar").gameObject.GetComponent<UIHealthBar> ();
			_healthBar.MaxValue = MaxLifetime;
		}
	}

	public void init () {

		if (!_initialised) {
			_joystick = GameObject.FindObjectOfType<Joystick> ();
            _gameManager = GameObject.FindObjectOfType<GameManager>();

			var markerScene = GameObject.Find ("Marker scene");
			if (markerScene != null) {
				transform.parent = markerScene.transform;
			} else {
				Debug.LogError ("Could not reposition car as child of Marker scene because we can't find Marker scene");
			}

			ApplyCarProperties ();

			if (!isServer) {
				if (GameObject.Find ("HealthBar") != null) {
					_healthBar = GameObject.Find ("HealthBar").gameObject.GetComponent<UIHealthBar> ();
					_healthBar.MaxValue = MaxLifetime;
				}
			}
            // The axes names are based on player number.

            _rigidbody = GetComponent<Rigidbody> ();
			Lifetime = MaxLifetime;
			_transferTime = Time.time;
			_initialised = true;
			_controlsDisabled = false;

			// hide and freeze so we can correctly position
			if (hasAuthority) {
				_rigidbody.isKinematic = true;
				gameObject.SetActive (false);
			}

			if (isServer){
				//_preparingGame = true;
				// register
				Debug.Log ("GameManager.Instance.AddCar (gameObject);");
				_gameManager.AddCar (gameObject);
			}

			if (_gameManager.WorldMesh != null) {
				Debug.Log ("available");
				Reposition (_gameManager.WorldMesh);
				SetFallDistance (_gameManager.WorldMesh);
			} else {
				Debug.Log ("unavailable");
				_gameManager.OnWorldMeshAvailableEvent += Reposition;
				_gameManager.OnWorldMeshAvailableEvent += SetFallDistance;
			}

			if (isLocalPlayer) {
				if (GameObject.Find ("ExplosionSound") != null) {
					ExplosionSound = GameObject.Find ("ExplosionSound").GetComponent<AudioSource> ();
				}
				if (GameObject.Find ("PowerUpSound") != null) {
					PowerUpSound = GameObject.Find ("PowerUpSound").GetComponent<AudioSource> ();
				}
				if (GameObject.Find ("BombAlertSound") != null) {
					BombAlertSound = GameObject.Find ("BombAlertSound").GetComponent<AudioSource> ();
				}
				if (GameObject.Find ("BumpSound") != null) {
					BumpSound = GameObject.Find ("BumpSound").GetComponent<AudioSource> ();
				}
				if (GameObject.Find ("PositiveSound") != null) {
					PositiveSound = GameObject.Find ("PositiveSound").GetComponent<AudioSource> ();	
				}
			}
				
		}

	}

	private void ApplyCarProperties () {

		var player = LobbyPlayer ();
		CarProperties.OriginalHue = player.colour;

		Garage garage = GameObject.FindObjectOfType<Garage> ();
		if (garage != null) {
			garage.ApplyVehicleToShell (player.vehicleId, gameObject, player.colour);
		} else {
			Debug.LogWarning ("couldn't find garage");
		}
	}

	void OnDestroy () {
		if (_gameManager != null) {
			_gameManager.OnWorldMeshAvailableEvent -= Reposition;
			_gameManager.OnWorldMeshAvailableEvent -= SetFallDistance;
		}
	}
		
	public void setBombAllDevices(bool b){
		RpcSetBomb (b);
		ServerSetBomb (b);
	}

	[ClientRpc]
	private void RpcSetBomb(bool b){
		setBomb (b);
	}

	[Server]
	private void ServerSetBomb(bool b){
		setBomb (b);
	}

	private void setBomb(bool hasBomb){
		HasBomb = hasBomb;
		if (hasBomb) {
			if (BombAlertSound != null) {
				BombAlertSound.PlayOneShot (BombAlertSound.clip);
			}
			CarProperties.SpeedLimit *= BombSpeedBoostFactor;
		} else {
			CarProperties.SpeedLimit /= BombSpeedBoostFactor;
			if (PositiveSound != null) {
				PositiveSound.PlayOneShot (PositiveSound.clip);
			}
		}
		#if UNITY_ANDROID || UNITY_IPHONE
		// vibrate on exchange
		if (isLocalPlayer){
			Handheld.Vibrate();
		}
		#endif
		if (OnSetBombEvent != null) {
			OnSetBombEvent (hasBomb);
		}
				
	}
		
	public void KillAllDevices(){
		RpcKill ();
		ServerKill ();
	}

	[ClientRpc]
	private void RpcKill(){
		if (hasAuthority) {
			_healthBar.Boom ();
			Debug.Log ("showdieddialog should be called in 5 sec");
			Invoke ("ShowDiedDialog", 4.3f);
		}
		Kill ();
	}

	private void ShowDiedDialog(){
		_gameManager.ShowDiedDialog ();
	}

	[Server]
	private void ServerKill(){
		Kill ();
	}

	private void Kill(){
		Lifetime = 0.0f;
		Alive = false;
		gameObject.SetActive (false);
		gameObject.transform.SetParent(GameObject.Find("InactiveObjects").transform);
		Boom ();
	}

	private void Boom(){
		Instantiate(ExplosionAnimation, transform.position, Quaternion.identity);
		if (ExplosionSound != null) {
			ExplosionSound.PlayOneShot (ExplosionSound.clip);
		}
	}

	private void Update ()
	{
		if (!_initialised)
			return;
				
		if ((isLocalPlayer || IsPlayingSolo)) {
            _healthBar.UpdateCountdown(Lifetime, HasBomb);
			EnsureCarIsOnMap ();
			transform.rotation = Quaternion.RotateTowards (transform.rotation, _lookAngle, CarProperties.SafeTurnRate * Time.deltaTime);
			if (Lifetime <= 0.0f) {
				Spectate ();
			}
		}

	}		

	private void FixedUpdate ()
	{
		transform.localScale = Vector3.one * CarProperties.SafeScale;

		if (!_initialised)
			return;
		
		if (!isLocalPlayer && !IsPlayingSolo)
		{ 
			return;
		}

		if (!_controlsDisabled) {
			Vector3 joystickVector = new Vector3 (_joystick.NormalisedVector.x, _joystick.NormalisedVector.y, 0);
			GameObject ARCamera = GameObject.Find ("ARCamera");
			Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

			if (_joystick.Active) {
				_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
				// think about combining z and y so that it moves away when close to 0 degrees
				float combined = _lookAngle.eulerAngles.y;
				_lookAngle.eulerAngles = new Vector3(0, combined, 0);
			
				// how close are we to facing the direction the user wants?
				float dirFactor = Mathf.Max(0,Vector3.Dot(transform.forward, _lookAngle * Vector3.forward));

				_rigidbody.AddForce (transform.forward * dirFactor * joystickVector.magnitude * CarProperties.SafeAccel);
			}

			if(_rigidbody.velocity.magnitude > CarProperties.SafeSpeedLimit) {
				_rigidbody.velocity = _rigidbody.velocity.normalized * CarProperties.SafeSpeedLimit;
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

    /*
     * Handling the PowerUp RPCs in the CarController because it makes it simpler to 
     * pass the relevant data to the right places (CarProperties).
     */
    [ClientRpc]
	public void RpcPowerUp(string tag, int triggeringServerId) {
		LocalPowerUp (tag, triggeringServerId);
    }

	public void LocalPowerUp (string tag, int triggeringServerId) {
		GamePowerUpManager gpm = _gameManager.PowerUpManager;
		AbilityRouter.RouteTag (tag, CarProperties, gameObject, gpm, triggeringServerId == ServerId, isLocalPlayer);
		if (PowerUpSound != null && triggeringServerId == ServerId) {
			PowerUpSound.PlayOneShot (PowerUpSound.clip);
		}
	}

    void OnCollisionEnter(Collision col)
	{
		if (isServer) {
			_gameManager.CollisionEvent (gameObject, col);
		}
		if (col.gameObject.CompareTag("Car")) {
			// Uncomment the following line to add bouncing between the players in the main game
			// the current implementation is a bit laggy so has been left uncommented until this is fixed 
			Bounce (col);
		}
	}

    private void OnTriggerEnter(Collider other)
    {
		_gameManager.TriggerEnterEvent(gameObject, other.transform.parent.gameObject);
    }

    void Bounce(Collision col){
		var bounceForce = 350;
		Vector3 direction = col.contacts[0].point - transform.position;
		direction = -direction.normalized;
		direction.y = 0;
		GetComponent<Rigidbody>().AddForce(direction * bounceForce);
		/*if (isLocalPlayer && BumpSound != null) {
			BumpSound.PlayOneShot (BumpSound.clip);
		}*/
	}


	public void Reposition(GameMapObjects gameMapObjects)
	{
		Debug.Log ("repositioning");
		if (hasAuthority) {
			Debug.Log ("repositioning with authority");

			Debug.Log ("Repositioning car");

			//Set velocities to zero
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;

			Vector3 position = GameUtils.FindSpawnLocationInsideConvexHull (gameMapObjects,0.8f);

			if (position != Vector3.zero) {
				Debug.Log ("unfreezing");
				// now unfreeze and show

				if (Alive == true) {
					gameObject.SetActive (true);
				}

				_rigidbody.isKinematic = false;

				_rigidbody.position = position;
			}

		}
	}
		
	public void EnsureCarIsOnMap(){
		if(_rigidbody.position.y <= _fallDistanceBeforeRespawn){
			Debug.Log ("Car Is not on map");
			Debug.Log ("car position: (" + _rigidbody.position.x + "," + _rigidbody.position.y + "," + _rigidbody.position.z + "), _fallDistanceBeforeRespawn: " + _fallDistanceBeforeRespawn );
			Reposition (_gameManager.WorldMesh);
			DisableControlsTime (DisabledControlDurationSeconds);
		}
	}

	public void DisableControlsTime(int seconds){
		DisableControls();
		Invoke("EnableControls", seconds);
	}
		

	public void EnableControls(){
		_controlsDisabled = false;
	}

	[ClientRpc]
	public void RpcEnableControls(){
		Debug.Log ("RPC CONTROLS");

		_controlsDisabled = false;
	}

	public void DisableControls(){
		_controlsDisabled = true;
	}

	private void DisablePlayerUI() {
		_joystick.gameObject.SetActive (false);
        _healthBar.gameObject.SetActive(false);
	}

	public void Spectate() {
		DisableControls ();
		DisablePlayerUI ();
		GameObject.Find ("SpectatingText").GetComponent<TextMeshProUGUI> ().text = "Spectating...";
	}

	private void SetFallDistance(GameMapObjects gameMapObjects){
		float meshHeight = gameMapObjects.ground.transform.GetComponent<MeshRenderer> ().bounds.size.y;
		float meshMinY = gameMapObjects.ground.transform.GetComponent<MeshRenderer> ().bounds.min.y;
		_fallDistanceBeforeRespawn = meshMinY - 2.0f;
		Debug.Log ("falldistance before spawn is set: " + _fallDistanceBeforeRespawn);
	}

}