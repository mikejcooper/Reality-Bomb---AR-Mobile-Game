using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class PlayerIndicationRenderer : MonoBehaviour
{

	public GameObject YouPointer;
	public GameObject BombObject;
	public GameObject EdgeGlow;
	public CarProperties carProperties;

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

	private Vector3 _youPointerUnitScale;
	private float _youPointerInitialScale;
	private float _transformInitialScale;

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


		authority = IsOwnCarProperties ();

		if (authority) {
			_youPointerUnitScale = new Vector3 (14.0f,60.0f,20.0f).normalized;
			_youPointerInitialScale = new Vector3(14.0f,60.0f,20.0f).magnitude * 2.0f;
			_transformInitialScale =  this.GetComponentInParent<CarProperties> ().SafeScale;

			_initialisedYouPointer = InstantiateAndDeactivate (YouPointer);
			_initialisedYouPointer.transform.localScale = _youPointerInitialScale * _youPointerUnitScale;
			_initialisedYouPointer.transform.localRotation = Quaternion.identity * Quaternion.Euler(-90,0,0);
			_initialisedYouPointer.SetActive (true);
		
			_initialisedGlow = InstantiateAndDeactivate (EdgeGlow);
			_glowImage = _initialisedGlow.GetComponent<Image> ();

			if (GameObject.FindObjectOfType<GameManager> () != null) {
				Debug.Log ("shrinkyoupointer is listening");
				GameObject.FindObjectOfType<GameManager> ().GameCountDownFinishedCallback += ShrinkYouPointer;	
			} else {
				Debug.Log ("gamemanager is null, shrinkyoupointer not listening");
				StartCoroutine (PollForGameManager ());
			}
		}
	}

	IEnumerator PollForGameManager()
	{
		while(GameObject.FindObjectOfType<GameManager>() == null) 
		{ 
			yield return new WaitForSeconds(0.5f);
		}
		Debug.Log ("shrinkyoupointer is listening from polling");
		GameObject.FindObjectOfType<GameManager> ().GameCountDownFinishedCallback += ShrinkYouPointer;
	}

	void ShrinkYouPointer ()
	{
		Debug.Log ("ShrinkYouPointerCalled");
		_youPointerInitialScale /= 2.0f;
		GameObject.FindObjectOfType<GameManager> ().GameCountDownFinishedCallback -= ShrinkYouPointer;	
	}



	private bool IsOwnCarProperties() {
		NetworkIdentity netWorkIdentitiy = this.GetComponentInParent<NetworkIdentity> ();
		return netWorkIdentitiy != null && netWorkIdentitiy.hasAuthority;// ( netWorkIdentitiy.clientAuthorityOwner == null || netWorkIdentitiy.hasAuthority);
//		return netWorkIdentitiy.hasAuthority;
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


	}


	void Update(){
		float sin = Mathf.PingPong (Time.time, 1.0f);
		Vector3 animationHeight = sin * 2 * new Vector3 (0.0f, 1.0f, 0.0f);
		if (isBombIndicaterOn) {

			if (_initialisedYouPointer != null) {
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 5.0f, 0.0f) + animationHeight;
				_initialisedYouPointer.transform.localScale = _youPointerUnitScale * (_transformInitialScale * _youPointerInitialScale) / this.GetComponentInParent<CarProperties> ().SafeScale;
			}
				

			float bombDangerLevel = 3 * (15.0f - _controller.Lifetime) / 15.0f;
			float lerp = Mathf.PingPong (bombDangerLevel * Time.time / 3.0f, 1.0f);
			_initialisedBomb.transform.GetChild (1).GetComponent<Renderer> ().material.Lerp (material1, material2, lerp);
			_initialisedBomb.transform.localScale =  (1.0f + lerp * 0.4f) * new Vector3 (80.0f, 80.0f, 80.0f);

			if (_glowImage != null) {
				var color = _glowImage.color;
				color.a = 0.5f + Mathf.PingPong (Time.time, 1.0f - 0.3f);
				_glowImage.color = color;
			}
		} else {
			if (_initialisedYouPointer != null) {
				_initialisedYouPointer.transform.localScale = _youPointerUnitScale * (_transformInitialScale * _youPointerInitialScale) / this.GetComponentInParent<CarProperties> ().SafeScale;
				_initialisedYouPointer.transform.localPosition = new Vector3 (0.0f, 3.6f, 0.0f) + animationHeight;
			}
		}
	}
		
	private GameObject InstantiateAndDeactivate (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(prefab);
		obj.transform.SetParent (transform, false);
		obj.transform.SetSiblingIndex (0);
		obj.SetActive (false);
		return obj;
	}

}

