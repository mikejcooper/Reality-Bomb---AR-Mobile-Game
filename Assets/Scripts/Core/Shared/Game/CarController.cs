using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class CarController : NetworkBehaviour
{
	public delegate void OnSetBomb (bool bomb);
	public event OnSetBomb OnSetBombEvent;

	// temporary hack to allow tank prefab to be spawned and played without a network system
	public bool IsPlayingSolo = false; 

	public CarProperties CarProperties;

	// How fast the tank turns in degrees per second.
	public float TurnSpeed = 180f;
	public float MaxLifetime = 60.0f;
	public bool HasBomb = false;
	public bool Alive = true;
	public int DisabledControlDurationSeconds = 2;
	// This is a server-only field that doesn't get updated on clients. I'll move this
	// at some point to a better place.
	public bool HasLoadedGame = false;
	public GameObject ExplosionAnimation;

	[SyncVar]
	public int ServerId;

	[SyncVar]
	public float Lifetime;

	private Joystick _joystick;
    private UIHealthBar _healthBar;
//    private Image _bombImage;
	// Reference used to move the tank.
	private Rigidbody _rigidbody;
	private Vector3 _direction;
	private Quaternion _lookAngle = Quaternion.Euler(Vector3.forward);
	private float _transferTime;
	private float _fallDistanceBeforeRespawn;


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
			_joystick = GameObject.FindObjectOfType<Joystick> ();
			Transform nameText = transform.Find ("NameTag").Find ("NameText");
			Text textComponent = nameText.gameObject.GetComponent<Text> ();
			if (isServer) {
				textComponent.text = ServerSceneManager.Instance.GetPlayerDataById (ServerId).Name;
			} else {
				textComponent.text = ClientSceneManager.Instance.GetPlayerDataById (ServerId).Name;
			}
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
			_controlsDisabled = true;

			// hide and freeze so we can correctly position
			if (hasAuthority) {
				_rigidbody.isKinematic = true;
				gameObject.SetActive (false);
			}

			if (isServer){
				//_preparingGame = true;
				// register
				Debug.Log ("GameManager.Instance.AddCar (gameObject);");
				GameObject.FindObjectOfType<GameManager> ().AddCar (gameObject);
			}

			if (GameObject.FindObjectOfType<GameManager> ().WorldMesh != null) {
				Debug.Log ("available");
				Reposition (GameObject.FindObjectOfType<GameManager> ().WorldMesh);
			} else {
				Debug.Log ("unavailable");
				GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent += Reposition;
				GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent += SetFallDiatance;
			}
				
		}

	}

	void OnDestroy () {
		GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent -= Reposition;
		GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent -= SetFallDiatance;

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

	private void setBomb(bool b){
		this.HasBomb = b;
//		#if UNITY_ANDROID || UNITY_IPHONE
//		if (isLocalPlayer && HasBomb != isBomb){
//			Handheld.Vibrate();
//		}
//		#endif
		if (OnSetBombEvent != null) {
			OnSetBombEvent (b);
		}
				
	}
		
	public void KillAllDevices(){
		RpcKill ();
		ServerKill ();
	}

	[ClientRpc]
	private void RpcKill(){
        if (hasAuthority)
            _healthBar.Boom();
		Kill ();
	}

	[Server]
	private void ServerKill(){
		Kill ();
	}

	private void Kill(){
		Lifetime = 0.0f;
		Alive = false;
		this.gameObject.SetActive (false);
		Boom ();
	}

	private void Boom(){
		Instantiate(ExplosionAnimation,transform.parent);
	}

	private void Update ()
	{
		if (!_initialised)
			return;
				
		if ((isLocalPlayer || IsPlayingSolo)) {
            _healthBar.UpdateCountdown(Lifetime, HasBomb);
			EnsureCarIsOnMap ();
			transform.rotation= Quaternion.Lerp (transform.rotation, _lookAngle , CarProperties.TurnRate * Time.deltaTime);
			if (Lifetime <= 0.0f) {
				Spectate ();
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
			Vector3 joystickVector = new Vector3 (_joystick.NormalisedVector.x, _joystick.NormalisedVector.y, 0);
			GameObject ARCamera = GameObject.Find ("ARCamera");
			Vector3 rotatedVector = ARCamera.transform.rotation * joystickVector;

			if (_joystick.Active) {
				_lookAngle = Quaternion.FromToRotation (Vector3.forward, rotatedVector);
				// think about combining z and y so that it moves away when close to 0 degrees
				float combined = _lookAngle.eulerAngles.y;
				_lookAngle.eulerAngles = new Vector3(0, combined, 0);
			}

			if (_joystick.Active) {
				_rigidbody.AddForce (transform.forward * joystickVector.magnitude * CarProperties.Acceleration);
			}

			if(_rigidbody.velocity.magnitude > CarProperties.MaxSpeed) {
				_rigidbody.velocity = _rigidbody.velocity.normalized * CarProperties.MaxSpeed;
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
		Debug.Log ("repositioning");
		if (hasAuthority) {
			Debug.Log ("repositioning with authority");

			Debug.Log ("Repositioning car");

			//Set velocities to zero
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.angularVelocity = Vector3.zero;

			Vector3 position = GameUtils.FindSpawnLocation (worldMesh);

			if (position != Vector3.zero) {
				Debug.Log ("unfreezing");
				// now unfreeze and show

				gameObject.SetActive (true);
				_rigidbody.isKinematic = false;

				_rigidbody.position = position;
			}

		}
	}
		
	public void EnsureCarIsOnMap(){
		if(_rigidbody.position.y <= _fallDistanceBeforeRespawn){
			Reposition (GameObject.FindObjectOfType<GameManager> ().WorldMesh);
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

	[ClientRpc]
	public void RpcStartGameCountDown(){
		Debug.Log ("RPC GAME COUNT DOWN");
		GameObject.FindObjectOfType<PreparingGame>().StartGameCountDown ();
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

	private void SetFallDiatance(GameObject _meshObj){
		float meshHeight = _meshObj.transform.GetComponent<MeshRenderer> ().bounds.size.y;
		float meshMinY = _meshObj.transform.GetComponent<MeshRenderer> ().bounds.min.y;
		_fallDistanceBeforeRespawn = meshMinY - meshHeight*0.65f;
	}

}