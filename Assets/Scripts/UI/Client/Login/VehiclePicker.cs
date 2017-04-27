using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehiclePicker : MonoBehaviour {

	public Carousel CarouselObj;
	public VehicleNameFlipper TextFlipperObj;
	public List<VehicleDefinition> AvailableVehicles;

	private int _currentVehicleIndex = 0;

	[System.Serializable]
	public class VehicleDefinition {
		public GameObject ModelPrefab;
		public string Name;
	}
		
	void Start () {

		// clear editor objects
		foreach (Transform child in CarouselObj.transform) {
			Destroy (child.gameObject);
		}

		for (int i=0; i<AvailableVehicles.Count; i++) {
			var vehicle = AvailableVehicles [i];
			GameObject.Instantiate (vehicle.ModelPrefab, CarouselObj.transform);
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

	public VehicleDefinition CurrentVehicle () {
		return AvailableVehicles [(_currentVehicleIndex + AvailableVehicles.Count*100) % AvailableVehicles.Count];
	}

}
