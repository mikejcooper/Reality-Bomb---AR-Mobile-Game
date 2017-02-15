using System;
using UnityEngine;


[Serializable]												// Allows object to be shown in Inspector.
public class MiniGameController
{
	// This class is to manage various settings on a tank.
	// It works with the GameManager class to control how the tanks behave
	// and whether or not players have control of their tank in the 
	// different phases of the game.

	// This is the color this tank will be tinted.
	public Color PlayerColor; 
	// The position and direction the tank will have when it spawns.
	public Vector3 SpawnPoint;
	// A string that represents the player with their number colored to match their tank.
	[HideInInspector] public string ColoredPlayerText;
	// A reference to the instance of the car when it is created.
	[HideInInspector] public GameObject CarObject;
	// The number of wins this player has so far.
	[HideInInspector] public int WinCount;


	// Reference to tank's movement script, used to disable and enable control.
	private CarController _movement;
	// Used to disable the world space UI during the Starting and Ending phases of each round.
	private GameObject _canvasGameObject;


	public void Setup ()
	{
		// Get references to the components.
		_movement = CarObject.GetComponent<CarController> ();

		_canvasGameObject = CarObject.GetComponentInChildren<Canvas> ().gameObject;

		// Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
		ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(PlayerColor) + ">PLAYER</color>";

		// Get all of the renderers of the car.
		MeshRenderer[] renderers = CarObject.GetComponentsInChildren<MeshRenderer> ();

		// Go through all the renderers...
		for (int i = 0; i < renderers.Length; i++)
		{
			// ... set their material color to the color specific to this tank.
			renderers[i].material.color = PlayerColor;
		}
	}


	// Used during the phases of the game where the player shouldn't be able to control their tank.
	public void DisableControl ()
	{
		_movement.enabled = false;
		_canvasGameObject.SetActive (false);
	}


	// Used during the phases of the game where the player should be able to control their tank.
	public void EnableControl ()
	{
		_movement.enabled = true;
		_canvasGameObject.SetActive (true);
	}


	// Used at the start of each round to put the tank into it's default state.
	public void Reset ()
	{
		CarObject.transform.position = SpawnPoint;
		CarObject.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));

		CarObject.SetActive (false);
		CarObject.SetActive (true);
	}
}