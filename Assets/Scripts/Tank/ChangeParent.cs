using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParent : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject ms = GameObject.Find("Marker scene");

        gameObject.transform.SetParent (ms.transform);
		SetLayerRecursively (gameObject, ms.layer);
	}

	void SetLayerRecursively(GameObject obj, int newLayer)
	{

		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
