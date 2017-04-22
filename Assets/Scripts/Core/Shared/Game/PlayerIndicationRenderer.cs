using System;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIndicationRenderer : MonoBehaviour
{

	public GameObject YouPointer;
	public GameObject BombObject;

	public bool isMeIndicaterOn = true;
	public bool isBombIndicaterOn = false;
	public Material material1;
	public Material material2;

	private bool authority = false;
	private GameObject _initialisedYouPointer;
	private GameObject _initialisedBomb;

	void Start(){
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
//		this.GetComponentInParent<CarController>().OnSetBombEvent -= setBombIndicator;
	}
		

	void setBombIndicator (bool bomb)
	{
		isBombIndicaterOn = bomb;
		_initialisedBomb.SetActive (bomb);
	}


		
	void Update(){
		if (authority && isMeIndicaterOn) {
			float sin = Mathf.PingPong (Time.time, 1.0f);
			Vector3 animationHeight = sin * 2 * new Vector3(0.0f,1.0f,0.0f);
			if (isBombIndicaterOn) {
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 5.5f, 0.0f) + animationHeight;
			} else {
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 2.5f, 0.0f) + animationHeight;
			}
			_initialisedYouPointer.transform.localScale = new Vector3(14.0f,60.0f,20.0f);
			_initialisedYouPointer.transform.localRotation = Quaternion.identity * Quaternion.Euler(-90,0,0);
			_initialisedYouPointer.transform.parent = transform;
			_initialisedYouPointer.SetActive (true);
		}
		if (isBombIndicaterOn) {
			CarController car = this.GetComponentInParent<CarController> ();

			float bombDangerLevel = 3.5f * (15.0f - car.Lifetime) / 15.0f;
			float lerp = Mathf.PingPong((bombDangerLevel * Time.time) / 3.0f, 1.0f) / 1.0f;

//			Debug.LogError(string.Format("bombDangerLevel: {0}, car.Lifetime: {1}, lerp: {2}", bombDangerLevel, car.Lifetime, lerp));

			_initialisedBomb.transform.localScale = new Vector3 (80.0f,80.0f,80.0f) + lerp * 20.0f * Vector3.one;
			_initialisedBomb.transform.localPosition = new Vector3 (0.0f,2.0f,0.0f);
			_initialisedBomb.transform.GetChild (1).GetComponent<Renderer> ().material.Lerp(material1,material2,lerp);
			_initialisedBomb.transform.parent = transform;
		}
	}
		
	private GameObject CreatePointer (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(prefab);
		obj.SetActive (false);
		return obj;
	}

}

