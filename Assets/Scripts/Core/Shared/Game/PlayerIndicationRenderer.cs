using System;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIndicationRenderer : MonoBehaviour
{

	public GameObject YouPointer;

	public bool isIndicaterOn = true;


	private bool authority = false;
	private GameObject _initialisedYouPointer;

	void Start(){
//		GameObject.FindObjectOfType<GameManager> ().OnGameStartedEvent += setIndicatorOff;
		authority = IsOwnCarProperties (GetComponent<CarProperties> ());
		if (authority) {
			_initialisedYouPointer = CreatePointer (YouPointer);
		} else {
			this.OnDestroy ();
		}
	}

	private bool IsOwnCarProperties(CarProperties properties) {
		var identity = properties.GetComponentInParent<NetworkIdentity> ();
//		return identity == null || identity.clientAuthorityOwner == null || identity.hasAuthority;
		return identity.hasAuthority;
	}

	void OnDestroy() {
//		GameObject.FindObjectOfType<GameManager> ().OnGameStartedEvent -= setIndicatorOff;
	}

	void setIndicatorOff ()
	{
		isIndicaterOn = false;
		_initialisedYouPointer.SetActive (false);
	}
		
	void Update(){
		if (authority && isIndicaterOn) {
			_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
			_initialisedYouPointer.transform.localScale = new Vector3(14.0f,60.0f,20.0f);
			_initialisedYouPointer.transform.localRotation = Quaternion.identity * Quaternion.Euler(-90,0,0);
			_initialisedYouPointer.transform.parent = transform;
			_initialisedYouPointer.SetActive (true);
		}
	}

	private GameObject CreatePointer (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(YouPointer);
		obj.SetActive (false);
		return obj;
	}

}

