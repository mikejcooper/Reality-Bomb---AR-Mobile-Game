using System;
using UnityEngine;


public class SandboxManager : MonoBehaviour
{
	public GameObject CarObject;
	public GameObject PlaneObject;


	void Start(){
		SpawnCar();
	}

	private void SpawnCar(){
		CarObject.transform.position = GameUtils.FindSpawnLocation (PlaneObject);
		CarObject.SetActive (true);
	}
}