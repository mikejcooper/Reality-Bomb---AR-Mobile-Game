using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSparkles : MonoBehaviour {

    Rigidbody _rbd;
    ParticleSystem.EmissionModule _emitter;

	// Use this for initialization
	void Start () {
        _rbd = transform.parent.gameObject.GetComponent<Rigidbody>();
        _emitter = GetComponent<ParticleSystem>().emission;
    }
	
	// Update is called once per frame
	void Update () {
        if (_rbd.velocity != Vector3.zero)
        {
            _emitter.enabled = true;
        }
        else
        {
            _emitter.enabled = false;
        }
	}
}
