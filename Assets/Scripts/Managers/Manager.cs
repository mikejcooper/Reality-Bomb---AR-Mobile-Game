using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_Tank;		//Reference the to tank object
	public Collision[] m_Cubes;		//Reference to the trigger zones (Target Cubes)
	public int m_ActiveCubeIndex;	//Index of the Currently Active Trigger
	private float m_TimeLeft;		//Remaining time to get to Trigger Zone
	private Text m_TimeLeftText;

	void Awake() {
		m_ActiveCubeIndex = 0;		//Initialises Cube0 as first trigger zone
		m_TimeLeft = 5.0f;			//Initialises time on clock to 5 secs
		m_TimeLeftText = GameObject.Find("TimeLeftText").gameObject.GetComponent<Text>();
		m_TimeLeftText.text = string.Format("{0:N2}", m_TimeLeft);
	}

	void Start() {
		m_Cubes [m_ActiveCubeIndex].rend.enabled = true;	//Sets the visibility of the active cube to true
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() {
		m_TimeLeft -= Time.deltaTime;	//Subtracts the elapsed time from the remaining time
		m_TimeLeftText.text = string.Format("{0:N2}", m_TimeLeft);

		if (m_TimeLeft < 0) {			//If time has expired call game over condition
			print ("Game Over!");
			Destroy (m_Tank);
		} else {						//If time has not yet expired
			print (m_TimeLeft);
			//if active cube is entered
			if (m_Cubes [m_ActiveCubeIndex].IsTriggered () == true) {	//If player has entered trigger zone
				print ("cube " + m_ActiveCubeIndex + " entered\n");
				m_Cubes [m_ActiveCubeIndex].rend.enabled = false;		//Disable visibility of current trigger
				//update active cube
				m_ActiveCubeIndex = (m_ActiveCubeIndex + 1) % m_Cubes.Length;	//Active the next trigger zone
				m_Cubes [m_ActiveCubeIndex].rend.enabled = true;		//Enable visibility of newly active trigger
				m_TimeLeft += 5.0f;		//Increment time left by 5 secs
			}
		}
	}
}

