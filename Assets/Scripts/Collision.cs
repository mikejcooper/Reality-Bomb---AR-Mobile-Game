using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {


	private bool m_Triggered;		//Stores wether the zone has been triggered or not
	public Renderer rend;			//Used to enable and disable visibility of trigger

	// Use this for initialization
	void Awake () {
		m_Triggered = false;		//Trigger initialised to false
		rend = GetComponent<Renderer> ();
		rend.enabled = false;		//Object visibility initially turned off
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {	//When player enters trigger zone
		m_Triggered = true;					//Trigger updated to true
	}

	void OnTriggerExit(Collider other) {
		m_Triggered = false;				//Trigger reset to false upon exiting the trigger zone
	}

	public bool IsTriggered() {
		return m_Triggered;					//Returns the triggered status of the zone
	}
}
