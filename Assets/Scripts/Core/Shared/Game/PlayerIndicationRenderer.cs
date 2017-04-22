﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerIndicationRenderer : MonoBehaviour
{

	public GameObject YouPointer;
	public GameObject BombObject;
	public GameObject EdgeGlow;

	public bool isMeIndicaterOn = true;
	public bool isBombIndicaterOn = false;
	public Material material1;
	public Material material2;

	private bool authority = false;
	private GameObject _initialisedYouPointer;
	private GameObject _initialisedBomb;
	private GameObject _initialisedGlow;

	private CarController _controller;
	private Image _glowImage;

	void Start(){
		Debug.Log("Starting Player Indication");
		if (this.GetComponentInParent<CarController> () != null) {
			Debug.Log ("CarController found");
			_controller = this.GetComponentInParent<CarController> ();
			_controller.OnSetBombEvent += setBombIndicator;
		}

		_initialisedBomb = InstantiateAndDeactivate (BombObject);
		_initialisedBomb.transform.localScale = new Vector3 (80.0f,80.0f,80.0f);
		_initialisedBomb.transform.localPosition = new Vector3 (0.0f,2.0f,0.0f);

		authority = IsOwnCarProperties (GetComponent<CarProperties> ());
		if (authority) {
			_initialisedYouPointer = InstantiateAndDeactivate (YouPointer);
			_initialisedYouPointer.transform.localScale = new Vector3(14.0f,60.0f,20.0f);
			_initialisedYouPointer.transform.localRotation = Quaternion.identity * Quaternion.Euler(-90,0,0);
			_initialisedYouPointer.SetActive (true);

			_initialisedGlow = InstantiateAndDeactivate (EdgeGlow);
			_glowImage = _initialisedGlow.GetComponent<Image> ();
		}
	}

	private bool IsOwnCarProperties(CarProperties properties) {
		var identity = properties.GetComponentInParent<NetworkIdentity> ();
//		return identity == null || identity.clientAuthorityOwner == null || identity.hasAuthority;
		return identity.hasAuthority;
	}

	void OnDestroy() {
//		this.GetComponentInParent<CarController>().OnSetBombEvent -= setBombIndicator;
	}

	void setMeIndicatorOff ()
	{
		isMeIndicaterOn = false;
		if (_initialisedYouPointer != null) {
			_initialisedYouPointer.SetActive (false);
		}
	}

	void setBombIndicator (bool hasBomb)
	{
		isBombIndicaterOn = hasBomb;
		_initialisedBomb.SetActive (hasBomb);

		if (_initialisedGlow != null) {
			_initialisedGlow.SetActive (hasBomb);
		}

//		if (_initialisedYouPointer != null) {
//			if (hasBomb) {
//				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 2.0f, 0.0f);
//			} else {
//				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
//			}
//		}


	}


	void Update(){
		float sin = Mathf.PingPong (Time.time, 1.0f);
		Vector3 animationHeight = sin * 2 * new Vector3 (0.0f, 1.0f, 0.0f);
		if (isBombIndicaterOn) {

			if(_initialisedYouPointer != null)
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 5.0f, 0.0f) + animationHeight;


			float bombDangerLevel = 3 * (15.0f - _controller.Lifetime) / 15.0f;
			float lerp = Mathf.PingPong (bombDangerLevel * Time.time / 3.0f, 1.0f);
			_initialisedBomb.transform.GetChild (1).GetComponent<Renderer> ().material.Lerp (material1, material2, lerp);
			_initialisedBomb.transform.localScale = (1.0f + lerp * 0.4f) * new Vector3 (80.0f, 80.0f, 80.0f);
			if (_glowImage != null) {
				var color = _glowImage.color;
				color.a = 0.3f + Mathf.PingPong (Time.time, 1.0f - 0.3f);
				_glowImage.color = color;
			}
		} else {
			if(_initialisedYouPointer != null)
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 2.2f, 0.0f) + animationHeight;
		}
	}
		
	private GameObject InstantiateAndDeactivate (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(prefab);
		obj.transform.SetParent (transform, false);
		obj.SetActive (false);
		return obj;
	}

}

