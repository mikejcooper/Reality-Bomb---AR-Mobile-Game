using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour {

	// Used to enable and disable visibility of trigger
	private Renderer _renderer;
	public float FallSpeed = 2.0f;
	public float SpinSpeed = 250.0f;

	// will store the type of power up (boost, invinsible, invisible, etc)
	public int P_Type;


//	private enum _powerUpList{
//		Speed,
//		Ink,
//		Other
//	}
//	public _powerUpList _powerUpType;



	private float _speedup;

	void Start () {
		gameObject.SetActive (true);
	}

	void Update() {
		if (gameObject.activeSelf) {
			if (transform.position.y > 7) {
				transform.Translate (Vector3.down * 10 * FallSpeed * Time.deltaTime, Space.World);
			}
			transform.Rotate (Vector3.forward, SpinSpeed * Time.deltaTime);
		}
	}

	// When player enters trigger zone
	void OnTriggerEnter(Collider player) {
		// below makes sphere invisible but it still exists in the scene ... needs to be deactivated altogether
		if (player.tag == "TankTag") {
			ActivatePowerUp (player, P_Type);
		}
		Invoke ("Kill", 0.2f);
	}

	void Kill() {
		gameObject.SetActive (false);
	}

	void ActivatePowerUp(Collider player, /*_powerUpList type*/ int type){
		player.gameObject.GetComponent<CarProperties> ().PowerUpActive = true;

//		if (type.Equals(_powerUpList.Speed)) {
//			print ("Speed boost activated!");
//			player.gameObject.GetComponent<CarProperties> ().Speed = 60.0f;
//		} else if (type.Equals(_powerUpList.Ink)) {
//			print ("Ink Splatter Activated!");
//		} else if (type.Equals(_powerUpList.Other)) {
//			print ("Some other powerup Activated");
//		}
//			

		if (type == 0) {	 // Speed Boost
			print ("Speed boost activated!");
			player.gameObject.GetComponent<CarProperties> ().Speed = 60.0f;
		} else if (type == 1) {		// Ink Splatter
			print ("Ink Splatter Activated!");
		} else if (type == 2) {		// Place Holder
			print ("Some other powerup Activated");
		}
		player.gameObject.GetComponent<CarProperties> ().PowerUpEndTime = Time.time + 5.0f;
	}
		
}
	