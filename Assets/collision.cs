using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision : MonoBehaviour {


	private bool triggered = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		Destroy (other.gameObject);
		triggered = true;
	}

	public bool IsTriggered() {
		return triggered;
	}

}
