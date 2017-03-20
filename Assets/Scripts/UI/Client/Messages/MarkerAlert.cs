using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerAlert : MonoBehaviour { 
   
    void OnMarkerFound(ARMarker marker)
    {
        Debug.Log("Marker Found");
        transform.GetChild(0).gameObject.SetActive(false);
    } 

    void OnMarkerLost(ARMarker marker)
    {
        Debug.Log("Marker Lost");
        transform.GetChild(0).gameObject.SetActive(true);
    }

    void OnMarkerTracked(ARMarker marker)
    {
        Debug.Log("Marker Tracked");
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
