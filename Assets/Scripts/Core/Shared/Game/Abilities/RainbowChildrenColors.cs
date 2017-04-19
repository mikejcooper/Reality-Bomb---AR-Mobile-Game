using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowChildrenColors : MonoBehaviour {

	private Component[] _renderers;

	void Start () {
		_renderers = gameObject.GetComponentsInChildren(typeof(Renderer));
		// Starting in Yf seconds,  FunctionZ will be called every Xf seconds
		InvokeRepeating("ChangeColor", 0.0f, 0.2f);
	}

	private void ChangeColor(){
		float hue = Random.Range(0,360);
		foreach(Renderer childRenderer in _renderers)
		{
			childRenderer.material.color = Color.HSVToRGB(hue/360f, 1.0f, 1.0f);
		}		
	}
				
}
