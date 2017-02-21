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
		
		GameObject powerUpObj = new GameObject ();
		PowerUp powerUp = powerUpObj.AddComponent<PowerUp> ();
		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);
		position.y += (_yOffSet + 200.0f);
		powerUpObj.transform.position = position;
		powerUp.P_Type = GenPowerUpType ();

		Collider col = powerUpObj.AddComponent<BoxCollider> ();
		col.isTrigger = true;


	}

	//Generate a Random Type for a powerup when spawning 
	private int GenPowerUpType () {
//		return (_powerUpList) Random.Range(0,3);
		return Random.Range(0,3);
	}




}


