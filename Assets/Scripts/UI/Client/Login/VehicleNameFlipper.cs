using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleNameFlipper : MonoBehaviour {

	public int FocusIndex;
	public Text TextObj;

	List<string> _nameList = new List<string>();

	public void AddName (string name) {
		_nameList.Add(name);
	}

	// Update is called once per frame
	void Update () {
		if (_nameList.Count > 0) {
			TextObj.text = _nameList [(FocusIndex + _nameList.Count * 100) % _nameList.Count];
		} else {
			TextObj.text = "namelist is empty!";
		}
	}

}
