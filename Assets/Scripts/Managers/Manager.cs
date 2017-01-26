using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;
	public Collision[] m_Cubes;
	public int m_ActiveCubeIndex;

	void Awake() {
		m_ActiveCubeIndex = 0;
	}

	void Start() {
		m_Cubes [m_ActiveCubeIndex].rend.enabled = true;
	}

	// This is called from start and will run each phase of the game one after another.
	void Update() {
		if (m_Cubes[m_ActiveCubeIndex].IsTriggered () == true) {
			print ("cube " + m_ActiveCubeIndex + " entered\n");
			m_Cubes [m_ActiveCubeIndex].rend.enabled = false;
			m_ActiveCubeIndex = (m_ActiveCubeIndex + 1) % m_Cubes.Length;
			m_Cubes [m_ActiveCubeIndex].rend.enabled = true;
		}
	}
}

