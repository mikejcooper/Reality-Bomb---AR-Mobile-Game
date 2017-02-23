using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class PowerUpManager : MonoBehaviour
{	
	public GameObject PlaneObject;
	private float _yOffSet;

	private enum _powerUpList{
		Speed,
		Ink,
		Other
	}

	void Start () {
		GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = false;
		OnMeshReady ();
	}
		
	void Update () {
		SpawnPowerUp ();
	}

	private void OnMeshReady ( ) {
		Bounds bounds = PlaneObject.transform.GetComponent<MeshRenderer> ().bounds;
		_yOffSet = bounds.size.y / 2.0f;
		GenPowerUp();
	}

	//Called every update to decide wether to randomly spawn a powerup or not
	private void SpawnPowerUp () {

		// Adjust Range Size to adjust spawn frequency
		int rand = Random.Range(0,250);

		// If generater produces the predetermined number from the range above, spawn a power up
		if (rand == 25) { 
			GenPowerUp ();
		}
	}

	// Generate a powerup once the decision to spawn one has been made
	private void GenPowerUp ( ) {
		
		GameObject powerUpObj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		PowerUp powerUp = powerUpObj.AddComponent<PowerUp> ();
		Collider collider = powerUpObj.GetComponent<SphereCollider> ();

		powerUpObj.name = "powerup";

		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);
		position.y += (_yOffSet + 200.0f);
		powerUpObj.transform.position = position;
		powerUpObj.transform.localScale = 10.0f * Vector3.one;

//		powerUp.P_Type = GenPowerUpType ();
		powerUp.SetPowerUpType(0);

		collider.isTrigger = true;


	}

	//Generate a Random Type for a powerup when spawning 
	private int GenPowerUpType () {
		return Random.Range(0,3);
	}




}


