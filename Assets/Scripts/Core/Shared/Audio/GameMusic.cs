using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMusic : MonoBehaviour {

	private AudioSource _music;
	public float FadeInTime;
	public float StartVolume;
	public float FinalVolume;
	public float FadeOutTime;

	// Use this for initialization
	void Start () {
		_music = GetComponent<AudioSource> ();
		_music.volume = StartVolume;
		DontDestroyOnLoad (gameObject);
		//StartCoroutine (FadeInMusic ());
		//GameObject.FindObjectOfType<GameManager> ().OnGameStartedEvent += StartMusic;
		SceneManager.activeSceneChanged += EndMusic; //Subscribe to event
	}



	IEnumerator FadeInMusic() {
		//Was getting error saying that the game object was destroyed??
		while (gameObject == null) {};
		print ("Fading in music...\n");
		float volumeStep = FinalVolume - StartVolume;
		_music.Play ();
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
		Destroy (gameObject);
	}

	void EndMusic(Scene previous, Scene next) {
		StartCoroutine (FadeOutMusic ());
	}

	public void StartMusic() {
		StartCoroutine (FadeInMusic ());
	}
}

