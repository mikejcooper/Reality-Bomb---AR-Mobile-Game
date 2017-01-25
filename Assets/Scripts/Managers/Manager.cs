using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;
	public GameObject m_Cube1;

	// This is called from start and will run each phase of the game one after another.
	private IEnumerator GameLoop ()
	{
		// Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
		yield return StartCoroutine (RoundStarting ());

	}
}

