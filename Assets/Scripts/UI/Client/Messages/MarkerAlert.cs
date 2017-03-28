using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerAlert : MonoBehaviour { 
   
    void OnMarkerFound(ARMarker marker)
    {
        Debug.Log("Marker Found");
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
    } 

    void OnMarkerLost(ARMarker marker)
    {
        Debug.Log("Marker Lost");
		foreach (Transform child in transform) {
			child.gameObject.SetActive (true);
		}
    }

    void OnMarkerTracked(ARMarker marker)
    {
        Debug.Log("Marker Tracked");
		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
    }
}
