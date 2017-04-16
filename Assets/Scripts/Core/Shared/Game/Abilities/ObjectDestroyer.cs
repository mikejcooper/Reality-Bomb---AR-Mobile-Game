using System;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour {

	public void DelayedDestroy(int delay) {
		Invoke ("DestroyNow", delay);
	}

	private void DestroyNow () {
		Destroy (gameObject);
	}

}

