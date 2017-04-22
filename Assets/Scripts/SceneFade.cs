using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFade : MonoBehaviour {

	public Texture2D FadeOutTexture;	// the texture that willl overlay the screen.
	public float FadeSpeed = 0.8f;		// the fading speed
	public bool FadeInScene;

	private int _drawDepth = -1000;		// the texture's order in the draw hierarchy: a low number renders on top
	private float _alpha = 1.0f;			// the texture's aplha value between 0 and 1
	private int _fadeDir = -1;			// the direction to fade: in = -1 or out = 1

	void Start() {
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		if (FadeInScene == false) {
			enabled = false;
		}
	}


	void OnGUI() {
		// fade out/in the alpha value using a direciton, a speed and Time.deltatime to convert the operation to seconds
		_alpha += _fadeDir * FadeSpeed * Time.deltaTime;
		// force (clamp) the number between 0 and 1 because GUI.color uses alpha values between 0 and 1
		_alpha = Mathf.Clamp01(_alpha);

		// set color of out GUI (in this case our texture). All color values remain the same & the Alpha is set to the alpha variable
		GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, _alpha);				// set alpha value
		GUI.depth = _drawDepth;																// make the black texture render on top (drawn last) 
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), FadeOutTexture);		// draw the texture to fit the entire screen area
	}

	// sets fadeDir to the direction parameter making the scene fade in -1 and out if 1
	public float BeginFade(int direction) {
		if (direction == 1) {
			_alpha = 0.0f;
		}
		_fadeDir = direction;
		return (FadeSpeed);
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
		BeginFade (-1);
	}
}
