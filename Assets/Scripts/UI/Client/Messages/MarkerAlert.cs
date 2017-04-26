using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerAlert : MonoBehaviour { 
   
    void OnMarkerFound(ARMarker marker)
    {
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
    } 

    void OnMarkerLost(ARMarker marker)
    {
		foreach (Transform child in transform) {
			child.gameObject.SetActive (true);
		}
    }

    void OnMarkerTracked(ARMarker marker)
    {
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
    }
}
