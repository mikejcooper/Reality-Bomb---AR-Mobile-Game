using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {


	private bool m_Triggered;
	public Renderer rend;

	// Use this for initialization
	void Awake () {
		m_Triggered = false;
		rend = GetComponent<Renderer> ();
		rend.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		m_Triggered = true;
	}

	void OnTriggerExit(Collider other) {
		m_Triggered = false;
	}

	public bool IsTriggered() {
		return m_Triggered;
	}
}
