using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FallingObject : MonoBehaviour {

	public Renderer rend;			//Used to enable and disable visibility of trigger
	public float fallSpeed = 60.0f;
	public float spinSpeed = 250.0f;
	private string type; // will store the type of power up (boost, invinsible, invisible, etc)

	private float speedup;

	// Use this for initialization
	void Start () {
		gameObject.SetActive (true);
	}

	void Update() {
		if (gameObject.activeSelf) {
			if (transform.position.y > 7) {
				transform.Translate (Vector3.down * 10*fallSpeed * Time.deltaTime, Space.World);
			}
			transform.Rotate (Vector3.forward, spinSpeed * Time.deltaTime);
		}
	}

	void OnTriggerEnter(Collider player) {	//When player enters trigger zone
		/******* below makes sphere invisible but it still exists in the scene ... needs to be deactivated altogether ********/
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
			player.gameObject.GetComponent<CarController> ().m_Speed = 60.0f;
		}

		player.gameObject.GetComponent<CarController> ().powerUpActive = true;
		player.gameObject.GetComponent<CarController> ().powerUpEndTime = Time.time + 5.0f;
	}
		
}

