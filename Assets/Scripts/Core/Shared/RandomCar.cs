using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCar : MonoBehaviour {

	public Garage Garage;

	private GameObject _car;

	void Start () {
		_car = Garage.InstantiateVehicle (Garage.CarType.MODEL, Garage.AvailableVehicles [Random.Range (0, Garage.AvailableVehicles.Count)].Id, Random.Range (0, 360));	
		_car.transform.SetParent (gameObject.transform, false);
	}
}
