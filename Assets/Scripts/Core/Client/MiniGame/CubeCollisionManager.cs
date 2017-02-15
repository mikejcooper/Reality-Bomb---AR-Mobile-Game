using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCollisionManager : MonoBehaviour {

	// Used to enable and disable visibility of trigger
	public Renderer Renderer;

	// Stores wether the zone has been triggered or not
	private bool _triggered;


	void Awake () {
		// Trigger initialised to false
		_triggered = false;
		Renderer = GetComponent<Renderer> ();
		// Object visibility initially turned off
		Renderer.enabled = false;
	}

	// When player enters trigger zone
	void OnTriggerEnter(Collider other) {	
		// Trigger updated to true
		_triggered = true;
	}

	void OnTriggerExit(Collider other) {
		// Trigger reset to false upon exiting the trigger zone
		_triggered = false;
	}

	public bool IsTriggered() {
		// Returns the triggered status of the zone
		return _triggered;
	}
}
