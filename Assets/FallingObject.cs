using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FallingObject : MonoBehaviour {

	public Renderer rend;			//Used to enable and disable visibility of trigger
	public float fallSpeed = 8.0f;
	public float spinSpeed = 250.0f;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();
		rend.enabled = true;
	}

	void Update() {

		transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
		transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

	}

	void OnTriggerEnter(Collider other) {	//When player enters trigger zone
		rend.enabled = false;
		print ("sphere hit tank");
	}

}

