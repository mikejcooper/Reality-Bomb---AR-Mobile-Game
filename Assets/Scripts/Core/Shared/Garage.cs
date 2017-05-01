using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garage : MonoBehaviour {

	public GameObject ModelCarShell;
	public GameObject OfflineCarShell;
	public GameObject OnlineCarShell;

	[SerializeField] 
	private List<VehicleEditorDefinition> _editorVehicles;

	[System.Serializable]
	public class VehicleEditorDefinition {
		public GameObject Model;
		public string Name;
	}

	public class Vehicle {
		public string Name;
		public int Id;

		public Vehicle (string name, int id) {
			Name = name; 
			Id = id;
		}
	}

	public enum CarType {
		MODEL,
		OFFLINE,
		ONLINE
	}


	public List<Vehicle> AvailableVehicles = new List<Vehicle>();

	private Dictionary<int, VehicleEditorDefinition> _idMap = new Dictionary<int, VehicleEditorDefinition>();

	void Awake () {
		AvailableVehicles = new List<Vehicle> ();
		int idCounter = 0;
		foreach (var vehicleDefinition in _editorVehicles) {
			var thisId = idCounter;
			_idMap [thisId] = vehicleDefinition;
			AvailableVehicles.Add (new Vehicle (vehicleDefinition.Name, idCounter));
			idCounter++;
		}
	}

	private GameObject GetShell(CarType type) {
		switch (type) {
		case CarType.MODEL:
			return ModelCarShell;
		case CarType.OFFLINE:
			return OfflineCarShell;
		case CarType.ONLINE:
			return OnlineCarShell;
		default:
			return null;
		}
	}

	public GameObject InstantiateVehicle(CarType type, int vehicleId, int hue) {
		var shell = GameObject.Instantiate (GetShell (type));
		ApplyVehicleToShell (vehicleId, shell, hue);
		return shell;
	}

	public GameObject InstantiateVehicle(CarType type, Vehicle vehicle, int hue) {
		return InstantiateVehicle (type, vehicle.Id, hue);
	}

	public void ApplyVehicleToShell(int vehicleId, GameObject shell, int hue) {
		var obj = GameObject.Instantiate (_idMap[vehicleId].Model, shell.transform);
		obj.transform.localPosition = new Vector3 (0.04f, -0.1f, 0.09f);
		obj.name = "Car_Model";
//		obj.transform.localScale = new Vector3 (2f, 3.1f, 1.8f);

		Material[] materials = obj.GetComponent<MeshRenderer> ().materials;

		GameUtils.SetCarMaterialColoursFromHue (materials, hue);

		obj.layer = LayerMask.NameToLayer ("Players");
	}

	public void ApplyVehicleToShell(Vehicle vehicle, GameObject shell, int hue) {
		ApplyVehicleToShell (vehicle.Id, shell, hue);
	}


}
