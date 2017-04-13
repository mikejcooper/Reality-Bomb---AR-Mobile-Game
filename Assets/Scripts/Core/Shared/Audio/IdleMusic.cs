using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleMusic : MusicFade {

	// Use this for initialization
	void Start () {
		_music = GetComponent<AudioSource> ();
		_music.volume = StartVolume;
		DontDestroyOnLoad (gameObject);
		StartMusic ();
		SceneManager.sceneUnloaded += StopMusic; //Subscribe to event
	}
}

