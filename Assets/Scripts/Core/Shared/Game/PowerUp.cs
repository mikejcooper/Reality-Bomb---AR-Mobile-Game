using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour {

	// Used to enable and disable visibility of trigger
	private Renderer _renderer;
//	public float FallSpeed = 2.0f;
	public float FallSpeed = 60.0f;
	public float SpinSpeed = 250.0f;
	public Image SplatterImg;

	// will store the type of power up (boost, invinsible, invisible, etc)
	private int P_Type;

	private Collider _playerCol;
	private float _powerUpEndTime;
	private float _powerUpDuration;

	void Start () {
		_powerUpEndTime = 0.0f;
		gameObject.SetActive (true);
	}

	void Update() {
		if (gameObject.activeSelf) {
			if (transform.position.y > 7) {
				transform.Translate (Vector3.down * 10 * FallSpeed * Time.deltaTime, Space.World);
			}
			transform.Rotate (Vector3.forward, SpinSpeed * Time.deltaTime);
		}

		if (Time.time > _powerUpEndTime) {
			Invoke ("DeactivatePowerUp", _powerUpDuration);
		}
	}

	// When player picks up a power up
	void OnTriggerEnter(Collider player) {
		_playerCol = player;
		if (_playerCol.tag == "TankTag") {
			ActivatePowerUp ();
		}
	}
		
	void ActivatePowerUp () {
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;

		if (P_Type == 0) {	 			// Speed Boost
			print ("Speed boost activated!");
			_playerCol.gameObject.GetComponent<CarProperties> ().Speed *= 2.0f;
			_powerUpDuration = 5.0f;
		} else if (P_Type == 1) {		// Ink Splatter
			print ("Ink Splatter Activated!");
			GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = true;
			_powerUpDuration = 5.0f;
		} else if (P_Type == 2) {		// Place Holder
			print ("Some other powerup Activated");
		}

		_powerUpEndTime = Time.time + 5.0f;
	}

	void DeactivatePowerUp () {
		print ("PowerUp Deactivated");

		if (P_Type == 0) {
//			_playerCol.gameObject.GetComponent<CarProperties> ().Speed *= 0.5f;
		} else if (P_Type == 1) {
			GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = false;
		}

		Destroy (this);
	}

	public void SetPowerUpType (int type) {
		P_Type = type;
	}
}
	