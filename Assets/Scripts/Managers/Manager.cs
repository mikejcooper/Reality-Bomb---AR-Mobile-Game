using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
	public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
	public TankManager m_Tank;		//Reference the to tank object
	public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.

	public Collision[] m_Cubes;		//Reference to the trigger zones (Target Cubes)
	public int m_ActiveCubeIndex;	//Index of the Currently Active Trigger
	private float m_TimeLeft;		//Remaining time to get to Trigger Zone
	private Text m_TimeLeftText;
	private int m_Score;
	private Text m_ScoreText;
	private bool m_GameOver;

	private float m_StartTime;


	void Awake() {
		m_ActiveCubeIndex = 0;		//Initialises Cube0 as first trigger zone
		m_TimeLeft = 5.0f;			//Initialises time on clock to 5 secs
		m_TimeLeftText = GameObject.Find("TimeLeftText").gameObject.GetComponent<Text>();  //Reference to Component which displays tiem remaining
		m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", m_TimeLeft); //Sets the time remaining text
		m_Score = 0;				//Initialises user score to 0
		m_ScoreText = GameObject.Find("ScoreText").gameObject.GetComponent<Text>(); //Reference to component displaying user score
		m_ScoreText.text = "Score: " + m_Score;	// Sets the Text for the user score
		m_StartTime = Time.time;
		m_GameOver = false;
	}

	void Start() {
		m_Cubes [m_ActiveCubeIndex].rend.enabled = true;	//Sets the visibility of the active cube to true
		SpawnTank();
		SetCameraTarget ();
		// Snap the camera's zoom and position to something appropriate for the reset tanks.
		m_CameraControl.SetStartPositionAndSize ();
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() {
		m_TimeLeft -= Time.deltaTime;	//Subtracts the elapsed time from the remaining time


		if (m_TimeLeft < 0) {			//If time has expired call game over condition
			if (m_GameOver == false) {
				m_GameOver = true;		
				m_StartTime = Time.time + 3.0f;
				m_TimeLeftText.text = "Time Left: " + string.Format ("{0:N2}", 5.0f);
				m_Score = 0;
				m_ScoreText.text = "Score: " + m_Score; //Updates the current user score
			}
			if (Time.time > m_StartTime) {
				m_TimeLeft = 5.0f;
				m_GameOver = false;
			}
			//ResetMiniGame(); 			// Calls routine to reset the level.
		} else {						//If time has not yet expired

			m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", m_TimeLeft); //Updates the remaining time
			m_ScoreText.text = "Score: " + m_Score; //Updates the current user score

			//if active cube is entered
			if (m_Cubes [m_ActiveCubeIndex].IsTriggered () == true) {	//If player has entered trigger zone
				print ("cube " + m_ActiveCubeIndex + " entered\n");
				m_Cubes [m_ActiveCubeIndex].rend.enabled = false;		//Disable visibility of current trigger
				//update active cube
				m_ActiveCubeIndex = (m_ActiveCubeIndex + 1) % m_Cubes.Length;	//Active the next trigger zone
				m_Cubes [m_ActiveCubeIndex].rend.enabled = true;		//Enable visibility of newly active trigger
				m_TimeLeft += 5.0f;		//Increment time left by 5 secs
				m_Score++;				//Increment user score
			}
		}
	}

	private void SpawnTank()
	{
		// For all the tanks...
			// ... create them, set their player number and references needed for control.
			m_Tank.m_Instance =
				Instantiate(m_TankPrefab, m_Tank.m_SpawnPoint.position, m_Tank.m_SpawnPoint.rotation) as GameObject;
			m_Tank.m_PlayerNumber = 0;
			m_Tank.Setup();
	}

	private void SetCameraTarget()
	{
		// Create a collection of transforms the same size as the number of tanks.
		Transform[] targets = new Transform[1];

		// ... set it to the appropriate tank transform.
		targets[0] = m_Tank.m_Instance.transform;

		// These are the targets the camera should follow.
		m_CameraControl.m_Targets = targets;
	}
}

