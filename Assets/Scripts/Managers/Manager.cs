using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;
	public Collision[] m_Cubes;
	public Collision m_ActiveCube;
	public int activeCube;

	void Start() {
		//m_ActiveCube = m_Cubes [0];
		activeCube = 0;
	}

	// This is called from start and will run each phase of the game one after another.
	void Update() {
		if (m_Cubes[activeCube].IsTriggered () == true) {
			print ("cube " + activeCube + " entered\n");
			activeCube = (activeCube + 1) % m_Cubes.Length;
			m_Cubes [activeCube].TriggerReset ();
		}
	}
}

