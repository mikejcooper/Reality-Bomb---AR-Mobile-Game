using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;


public class PowerUpManager : MonoBehaviour
{	
	public GameObject PlaneObject;
	private float _yOffSet;
	public Canvas PlayerCanvas;
	public Texture SplatterTexture;

	private enum _powerUpList{
		Speed,
		Ink,
		Other
	}

	void Start () {
//		GameObject.FindGameObjectWithTag("Splatter").GetComponent<UnityEngine.UI.RawImage>().enabled = false;
		OnMeshReady ();
	}

	// use coroutines rather than running on every update
	IEnumerator tryToSpawn()
	{
		while(true) 
		{ 
			// Adjust Range Size to adjust spawn frequency
			int rand = Random.Range(0,5);

			// If generater produces the predetermined number from the range above, spawn a power up
			if (rand == 0) { 
				GenPowerUp ();
			}

			yield return new WaitForSeconds(5);
		}
	}

	private void OnMeshReady ( ) {
		Bounds bounds = PlaneObject.transform.GetComponent<MeshRenderer> ().bounds;
		_yOffSet = bounds.size.y / 2.0f;

		StartCoroutine (tryToSpawn());
	}


	// Generate a powerup once the decision to spawn one has been made
	private void GenPowerUp ( ) {
		
		GameObject powerUpObj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		Rigidbody rigidBody = powerUpObj.AddComponent<Rigidbody> ();
		PowerUp powerUp = powerUpObj.AddComponent<PowerUp> ();
		Collider collider = powerUpObj.GetComponent<SphereCollider> ();

		powerUpObj.name = "powerup";


		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);
		position.y += (_yOffSet + 100.0f);
		powerUpObj.transform.position = position;
		powerUpObj.transform.localScale = 10.0f * Vector3.one;

		powerUp.SetPowerUpType (0);//GenPowerUpType ());
		powerUp.PlayerCanvas = PlayerCanvas;
		powerUp.SplatterTex = SplatterTexture;


	}

	//Generate a Random Type for a powerup when spawning 
	private int GenPowerUpType () {
		return Random.Range(0,3);
	}




}


