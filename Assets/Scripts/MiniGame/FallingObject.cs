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
		rend = GetComponent<Renderer> ();
		rend.enabled = true;
	}

	void Update() {
		if(transform.position.y > 7) {
			transform.Translate (Vector3.down * fallSpeed * Time.deltaTime, Space.World);
		}
		transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

	}

	void OnTriggerEnter(Collider player) {	//When player enters trigger zone
		rend.enabled = false;


		if (player.tag == "TankTag") {
			ActivatePowerUp (player, "Speed");

			//TODO finish powerup decactivation

			//			StartCoroutine(DeactivatePowerUp (player, "Speed"));
		}
	}

	void ActivatePowerUp(Collider player, string str){
		if (str.Equals ("Speed")) {
			print ("Speed boost activated!");
			player.gameObject.GetComponent<TankController> ().m_Speed = 60;
		}
	}

//	//Trying to deactive powerup after 5 secs
//	IEnumerator DeactivatePowerUp(Collider player, string str){
//		yield return WaitForSeconds (5.0f);
//		if (str.Equals("Speed"))
//			player.gameObject.GetComponent<TankController> ().m_Speed = 30;
//		return;
//	}
}

