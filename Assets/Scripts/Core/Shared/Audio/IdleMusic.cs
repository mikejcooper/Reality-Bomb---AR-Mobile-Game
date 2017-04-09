using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleMusic : MonoBehaviour {

	private AudioSource _music;
	public float FadeInTime;
	public float StartVolume;
	public float FinalVolume;
	public float FadeOutTime;

	// Use this for initialization
	void Start () {
		_music = GetComponent<AudioSource> ();
		_music.volume = StartVolume;
		//DontDestroyOnLoad (gameObject);
		StartCoroutine (FadeInMusic ());
		//SceneManager.activeSceneChanged += MyMethod; //Subscribe to event
	}

	IEnumerator FadeInMusic() {
		//Was getting error saying that the game object was destroyed??
		while (gameObject == null) {};
		print ("Fading in music...\n");
		float volumeStep = FinalVolume - StartVolume;
		while (_music.volume < FinalVolume) {
			_music.volume += (volumeStep * Time.deltaTime) / FadeInTime;
			yield return null;
		}
	}

	IEnumerator FadeOutMusic() {
		if (FadeOutTime > 0) {
			while (_music.volume > 0) {
				_music.volume -= FinalVolume * Time.deltaTime / FadeOutTime;
				yield return null;
			}
		} 
		else {
			_music.Stop ();
		}
		_music.Stop ();
		SceneManager.activeSceneChanged -= MyMethod; //Unsubscribe from event
		Destroy (gameObject);
	}

	void MyMethod(Scene previous, Scene next) {
		StartCoroutine (FadeOutMusic ());
	}
}

