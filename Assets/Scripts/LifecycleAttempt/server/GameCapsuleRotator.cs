using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameCapsuleRotator : NetworkBehaviour {

	public GameObject capsule;
	private int theta = 0;


	[SyncVar]
	private Vector3 pos; // this variable can be anything. So long as it's constantly updated then Networktransforms seem to work

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (hasAuthority) {
			pos = new Vector3 (Mathf.Sin (Mathf.Deg2Rad * theta), 0, Mathf.Cos (Mathf.Deg2Rad * theta));
			theta += (int)(100 * Time.deltaTime);
			capsule.transform.position = pos;

		} else {
			Debug.Log ("We don't have authority");
		}



	}
}
