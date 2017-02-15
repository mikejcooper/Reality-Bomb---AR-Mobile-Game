using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FallingObject : MonoBehaviour {

	// Used to enable and disable visibility of trigger
	private Renderer _renderer;
	public float FallSpeed = 60.0f;
	public float SpinSpeed = 250.0f;
	// will store the type of power up (boost, invinsible, invisible, etc)
	private string _powerType; 

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
			ActivatePowerUp (player, "Speed");
		}
		Invoke ("Kill", 0.2f);
	}

	void Kill() {
		gameObject.SetActive (false);
	}

	void ActivatePowerUp(Collider player, string str){
		if (str.Equals ("Speed")) {
			print ("Speed boost activated!");
			player.gameObject.GetComponent<OfflineCarController> ().Speed = 60.0f;
		}

		// todo: these need to be changed to use a Singleton TankController rather than searching for
		// the TankController
		player.gameObject.GetComponent<OfflineCarController> ().PowerUpActive = true;
		player.gameObject.GetComponent<OfflineCarController> ().PowerUpEndTime = Time.time + 5.0f;
	}
		
}

