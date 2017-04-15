using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicFade : MonoBehaviour {

	protected AudioSource _music;
	public float FadeInTime;
	public float StartVolume;
	public float FinalVolume;
	public float FadeOutTime;

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
		SceneManager.sceneUnloaded -= StopMusic;
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

	public void StopMusic(Scene a) {
		StartCoroutine (FadeOutMusic ());
	}

	public void StartMusic() {
		StartCoroutine (FadeInMusic ());
	}
}

