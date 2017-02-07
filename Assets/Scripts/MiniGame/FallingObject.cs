using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FallingObject : MonoBehaviour {

	public Renderer rend;			//Used to enable and disable visibility of trigger
	public float fallSpeed = 60.0f;
	public float spinSpeed = 250.0f;
	private string type; // will store the type of power up (boost, invinsible, invisible, etc)

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

	void OnTriggerEnter(Collider other) {	//When player enters trigger zone
		rend.enabled = false;
		print ("sphere hit tank");

		// switch statement based on the power up type to match relevant action of powerup
	}

}

