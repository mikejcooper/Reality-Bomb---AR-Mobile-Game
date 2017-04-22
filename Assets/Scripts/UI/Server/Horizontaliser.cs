using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horizontaliser : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 eulers = transform.eulerAngles;
        eulers.z = 0;
        transform.eulerAngles = eulers;
    }
}
