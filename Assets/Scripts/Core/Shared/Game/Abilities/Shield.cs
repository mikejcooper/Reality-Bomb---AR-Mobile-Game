using System;
using UnityEngine;


public class Shield  : MonoBehaviour {

	public GameObject ShieldPrefab;

	private GameObject _shieldObject;

	void Start () {
		// Instantiate and keep track of the new instance
	    _shieldObject = Instantiate(ShieldPrefab);

		// Set parent
		_shieldObject.transform.SetParent(transform, false);
		_shieldObject.transform.localPosition = Vector3.zero;


		var rend = _shieldObject.GetComponent <MeshRenderer> ();
		Color shieldColour = new Color (1.0f, 1.0f, 0.0f, 0.35f);
		rend.material.color = shieldColour;
		rend.material.shader = Shader.Find( "Transparent/Diffuse" );

	}

	void OnTriggerEnter(Collider other) {
		if (other.tag != "PowerUp") {
			var force = 600;
			Vector3 explosionPos = transform.position;
			Rigidbody rb = other.GetComponent<Rigidbody>();
			if (rb != null)
				rb.AddExplosionForce(force, explosionPos, 3.0f, 0.0f);
			
		}
	}

	public void Destroy(){
		Destroy(_shieldObject);
	}



}

