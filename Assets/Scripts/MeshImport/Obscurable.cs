using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obscurable : MonoBehaviour {

	// Use this for initialization

	void Start () {
		var renders = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renders){
			renderer.material.renderQueue = 2002; // set their renderQueue
		}
	}	
		
}
