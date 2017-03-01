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

	private string _gui_Txt = "Welcome to the Tutorial!\nUse the joystick to drive around but don't fall off the map!\nSee if you can pickup some Power Ups along the way!";
	private enum _powerUpList{
		Speed,
		Ink,
		Other
	}

	void Start () {
		OnMeshReady ();
		Invoke ("ClearGuiTxt", 10.0f);
	}

	void OnGUI(){
		GUI.Label(new Rect(Screen.width/2.0f - 150,Screen.height/40.0f, 800, 800), _gui_Txt);
	}

	// use coroutines rather than running on every update
	IEnumerator TryToSpawn()
	{
		while(true) 
		{ 
			// Adjust Range Size to adjust spawn frequency
			int rand = Random.Range(0,5);

			// If generater produces the predetermined number from the range above, spawn a power up
			if (rand == 0/*|| rand == 1 || rand == 2*/) { 
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
		position.y += (_yOffSet + 100.0f);
		powerUpObj.transform.position = position;
		powerUpObj.transform.localScale = 10.0f * Vector3.one;

		powerUp.SetPowerUpType (1);//GenPowerUpType ());
		powerUp.PlayerCanvas = PlayerCanvas;
		powerUp.SplatterTex = SplatterTexture;


	}

	//Generate a Random Type for a powerup when spawning 
	private int GenPowerUpType () {
		return Random.Range(0,3);
	}

	private void ClearGuiTxt(){
		_gui_Txt = "";
	}


}


