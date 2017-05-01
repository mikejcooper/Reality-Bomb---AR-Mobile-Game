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

	/*void RaisePitch() {
		_music.pitch *= 1.1f;
	}*/
}

