using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMusic : MusicFade {

	// Use this for initialization
	void Start () {
		_music = GetComponent<AudioSource> ();
		_music.volume = StartVolume;
		DontDestroyOnLoad (gameObject);
		//Starting of the game music is called in the gamemanager
		SceneManager.sceneUnloaded += StopMusic; //Subscribe to event
	}

	public void SetPitch(int noAlivePlayers) {
		if (noAlivePlayers <= 5) {
			switch (noAlivePlayers) {
			case 5:
				_music.pitch = 1.05f;
				break;
			case 4:
				_music.pitch = 1.1f;
				break;
			case 3:
				_music.pitch = 1.15f;
				break;
			case 2:
				_music.pitch = 1.2f;
				break;
			}
		}
	}
}

