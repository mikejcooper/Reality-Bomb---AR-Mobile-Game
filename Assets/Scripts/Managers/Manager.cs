using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;
	public Collision[] m_Cubes;
	public int m_ActiveCubeIndex;
	private float m_TimeLeft;

	void Awake() {
		m_ActiveCubeIndex = 0;
		m_TimeLeft = 5.0f;
	}

	void Start() {
		m_Cubes [m_ActiveCubeIndex].rend.enabled = true;
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() {
		m_TimeLeft -= Time.deltaTime;

		if (m_TimeLeft < 0) {
			print ("Game Over!");
			Destroy (m_Tank);
		} else {
			print (m_TimeLeft);
			//if active cube is entered
			if (m_Cubes [m_ActiveCubeIndex].IsTriggered () == true) {
				print ("cube " + m_ActiveCubeIndex + " entered\n");
				m_Cubes [m_ActiveCubeIndex].rend.enabled = false;
				//update active cube
				m_ActiveCubeIndex = (m_ActiveCubeIndex + 1) % m_Cubes.Length;
				m_Cubes [m_ActiveCubeIndex].rend.enabled = true;
				m_TimeLeft += 5.0f;
			}
		}
	}
}

