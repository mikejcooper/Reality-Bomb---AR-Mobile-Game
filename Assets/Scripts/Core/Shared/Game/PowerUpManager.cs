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

//	private bool _controlsDisabled;

	private enum _powerUpList{
		Speed,
		Ink,
		Other
	}

	void Start () {
		OnMeshReady ();
//		_controlsDisabled = false;

	}


	// use coroutines rather than running on every update
	IEnumerator TryToSpawn()
	{
		while(true) 
		{ 
			// Adjust Range Size to adjust spawn frequency
			int rand = Random.Range(0,5);

			// If generater produces the predetermined number from the range above, spawn a power up
			if (rand == 0|| rand == 1 || rand == 2/**/) { 
				GenPowerUp ();
			}

			yield return new WaitForSeconds(5);
		}
	}

	private void OnMeshReady ( ) {
		Bounds bounds = PlaneObject.transform.GetComponent<MeshRenderer> ().bounds;
		_yOffSet = bounds.size.y / 2.0f;

		StartCoroutine (TryToSpawn());
	}


	// Generate a powerup once the decision to spawn one has been made
	private void GenPowerUp ( ) {
		
		GameObject powerUpObj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		Rigidbody rigidBody = powerUpObj.AddComponent<Rigidbody> ();
		PowerUp powerUp = powerUpObj.AddComponent<PowerUp> ();
		Collider collider = powerUpObj.GetComponent<SphereCollider> ();

		powerUpObj.name = "powerup";


		Vector3 position = GameUtils.FindSpawnLocation (PlaneObject);
		position.y += (_yOffSet + 10.0f);
		powerUpObj.transform.position = position;
		powerUpObj.transform.localScale = Vector3.one;

		powerUp.SetPowerUpType (1);//GenPowerUpType ());
		powerUp.PlayerCanvas = PlayerCanvas;
		powerUp.SplatterTex = SplatterTexture;

		powerUp.SetPowerUpManager (this);
	}

	//Generate a Random Type for a powerup when spawning 
	private int GenPowerUpType () {
		return Random.Range(0,3);
	}

	public delegate void OnSplatterStart ();
	public delegate void OnSplatterEnd ();
	public delegate void OnSpeedUpStart ();
	public delegate void OnSpeedUpEnd ();

	public event OnSplatterStart OnSplatterStartEvent = delegate {};
	public event OnSplatterEnd OnSplatterEndEvent = delegate {};
	public event OnSpeedUpStart OnSpeedUpStartEvent = delegate {};
	public event OnSpeedUpEnd OnSpeedUpEndEvent = delegate {};
}


