using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour {

	// Used to enable and disable visibility of trigger
	private Renderer _renderer;
	public float FallSpeed = 2.0f;
	public float SpinSpeed = 250.0f;
	public Image SplatterImg;

	// will store the type of power up (boost, invinsible, invisible, etc)
	public int P_Type;


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

	// When player picks up a power up
	void OnTriggerEnter(Collider player) {
		if (player.tag == "TankTag") {
			ActivatePowerUp (player, P_Type);
			if (P_Type == 1) {
				Invoke ("RemoveSplatter", 5.0f);
			}
		}
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;
	}

	void RemoveSplatter() {
		GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = false;
		Destroy (this);
	}

	void ActivatePowerUp(Collider player, int type){
		player.gameObject.GetComponent<CarProperties> ().PowerUpActive = true;


		if (type == 0) {	 // Speed Boost
			print ("Speed boost activated!");
			player.gameObject.GetComponent<CarProperties> ().Speed = 60.0f;
		} else if (type == 1) {		// Ink Splatter
			print ("Ink Splatter Activated!");
			GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = true;
		} else if (type == 2) {		// Place Holder
			print ("Some other powerup Activated");
		}

		player.gameObject.GetComponent<CarProperties> ().PowerUpEndTime = Time.time + 5.0f;
	}
		
}
	