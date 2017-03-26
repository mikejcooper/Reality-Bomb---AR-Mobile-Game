using System;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIndicationRenderer : MonoBehaviour
{

	public GameObject YouPointer;
	public GameObject BombObject;

	public bool isMeIndicaterOn = true;
	public bool isBombIndicaterOn = true;


	private bool authority = false;
	private GameObject _initialisedYouPointer;
	private GameObject _initialisedBomb;

	void Start(){
//		GameObject.FindObjectOfType<GameManager> ().OnGameStartedEvent += setIndicatorOff;
		Debug.Log("Starting Player Indication");
		if (this.GetComponentInParent<CarController> () != null) {
			Debug.Log ("CarController found");
			this.GetComponentInParent<CarController>().OnSetBombEvent += setBombIndicator;
		}
		_initialisedBomb = CreatePointer (BombObject);
		authority = IsOwnCarProperties (GetComponent<CarProperties> ());
		if (authority) {
			_initialisedYouPointer = CreatePointer (YouPointer);
		}
	}

	private bool IsOwnCarProperties(CarProperties properties) {
		var identity = properties.GetComponentInParent<NetworkIdentity> ();
//		return identity == null || identity.clientAuthorityOwner == null || identity.hasAuthority;
		return identity.hasAuthority;
	}

	void OnDestroy() {
//		GameObject.FindObjectOfType<GameManager> ().OnGameStartedEvent -= setIndicatorOff;
//		this.GetComponentInParent<CarController>().OnSetBombEvent -= setBombIndicator;
	}

	void setMeIndicatorOff ()
	{
		isMeIndicaterOn = false;
		_initialisedYouPointer.SetActive (false);
	}

	void setBombIndicator (bool bomb)
	{
		isBombIndicaterOn = bomb;
		_initialisedBomb.SetActive (bomb);
	}


		
	void Update(){
		if (authority && isMeIndicaterOn) {
			if (isBombIndicaterOn) {
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 2.0f, 0.0f);
			} else {
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
			}
			_initialisedYouPointer.transform.localScale = new Vector3(14.0f,60.0f,20.0f);
			_initialisedYouPointer.transform.localRotation = Quaternion.identity * Quaternion.Euler(-90,0,0);
			_initialisedYouPointer.transform.parent = transform;
			_initialisedYouPointer.SetActive (true);
		}
		if (isBombIndicaterOn) {
			_initialisedBomb.transform.parent = transform;
			_initialisedBomb.transform.localScale = new Vector3 (80.0f,80.0f,80.0f);
			_initialisedBomb.transform.localPosition = new Vector3 (0.0f,2.0f,0.0f);
		}
	}

	private GameObject CreatePointer (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(prefab);
		obj.SetActive (false);
		return obj;
	}

}

