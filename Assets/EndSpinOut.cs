using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSpinOut : MonoBehaviour {
    public delegate void OnEndSpinOut();
    public event OnEndSpinOut OnEndSpinOutEvent;
    
    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public void OnEndSpinOutCaller()
    {
        Debug.Log("Calling event");
        OnEndSpinOutEvent();
    }

}
