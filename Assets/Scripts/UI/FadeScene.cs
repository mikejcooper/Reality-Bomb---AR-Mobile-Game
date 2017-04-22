using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeScene : MonoBehaviour {

	private Image _image;

	public bool FadeInScene;
	public float FadeInTime;
	public float FadeOutTime;

	// Use this for initialization
	void Start () {
		_image = GetComponent<Image>();
		transform.SetAsFirstSibling ();
		if (FadeInScene) {
			transform.SetAsLastSibling ();
			Color temp;
			temp = _image.color;
			temp.a = 1.0f;
			_image.color = temp;
			BeginFadeIn ();
		}
	}
	
	IEnumerator FadeIn() {
		Color temp;
		print ("Fading in scene");
		while (_image.color.a > 0.0f) {
			temp = _image.color;
			temp.a -= (1.0f / FadeInTime) * Time.deltaTime;
			_image.color = temp;
			yield return null;
		}
		temp = _image.color;
		temp.a = 0.0f;
		_image.color = temp;
		transform.SetAsFirstSibling ();
	}

	IEnumerator FadeOut() {
		transform.SetAsLastSibling ();
		print ("Fading out scene");
		Color temp;
		while (_image.color.a < 1.0f) {
			temp = _image.color;
			temp.a += (1.0f / FadeOutTime) * Time.deltaTime;
			_image.color = temp;
			yield return null;
		}
		temp = _image.color;
		temp.a = 1.0f;
		_image.color = temp;
	}
		
	public void BeginFadeIn() {
		StartCoroutine (FadeIn ());
	}

	public float BeginFadeOut() {
		StartCoroutine (FadeOut ());
		return FadeOutTime;
	}
}
