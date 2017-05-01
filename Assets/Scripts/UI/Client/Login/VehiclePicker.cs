using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehiclePicker : MonoBehaviour {

	public Carousel CarouselObj;
	public VehicleNameFlipper TextFlipperObj;
	public Garage Garage;

	private int _currentVehicleIndex = 0;

		
	void Start () {

		// clear editor objects
		foreach (Transform child in CarouselObj.transform) {
			Destroy (child.gameObject);
		}

		for (int i=0; i<Garage.AvailableVehicles.Count; i++) {
			var vehicle = Garage.AvailableVehicles [i];
			var obj = Garage.InstantiateVehicle(Garage.CarType.MODEL, vehicle, 320);
			obj.transform.SetParent (CarouselObj.transform, false);
			TextFlipperObj.AddName (vehicle.Name);
		}
	}

	public void MoveLeft () {
		_currentVehicleIndex++;
		CarouselObj.FocusIndex = _currentVehicleIndex;
		TextFlipperObj.FocusIndex = _currentVehicleIndex;
	}

	public void MoveRight () {
		_currentVehicleIndex--;
		CarouselObj.FocusIndex = _currentVehicleIndex;
		TextFlipperObj.FocusIndex = _currentVehicleIndex;
	}

	public Garage.Vehicle CurrentVehicle () {
		return Garage.AvailableVehicles [(_currentVehicleIndex + Garage.AvailableVehicles.Count*100) % Garage.AvailableVehicles.Count];
	}

}
