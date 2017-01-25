using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;
	public Collision m_Cube1;

	// This is called from start and will run each phase of the game one after another.
	void Update() {
		if (m_Cube1.IsTriggered() == true) {
			Destroy (m_Tank);
		}
	}
}

