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

	}

    //This was causing collisions with the plane. I put the shield in a different layer to avoid this.
	void OnTriggerEnter(Collider other) {
		if (!Abilities.AbilityRouter.IsAbilityObject(other.gameObject)) {
			var force = 600;
			Vector3 explosionPos = transform.position;
			Rigidbody rb1 = other.GetComponent<Rigidbody>();
			if (rb1 != null) {
				rb1.AddExplosionForce (force, explosionPos, 3.0f, 0.0f);

			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (!Abilities.AbilityRouter.IsAbilityObject(other.gameObject)) {
			var force = 600;
			Vector3 explosionPos = transform.position;
			Rigidbody rb2 = gameObject.GetComponent<Rigidbody>();
			if (rb2 != null) {
				rb2.AddExplosionForce (force, -explosionPos, 3.0f, 0.0f);
			}
		}
	}
    

	public void OnDestroy(){
		Destroy(_shieldObject);
	}



}

