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
	private int m_Score;
	private Text m_ScoreText;
	private bool m_GameOver;

	void Awake() {
		m_ActiveCubeIndex = 0;		//Initialises Cube0 as first trigger zone
		m_TimeLeft = 5.0f;			//Initialises time on clock to 5 secs
		m_TimeLeftText = GameObject.Find("TimeLeftText").gameObject.GetComponent<Text>();  //Reference to Component which displays tiem remaining
		m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", m_TimeLeft); //Sets the time remaining text
		m_Score = 0;				//Initialises user score to 0
		m_ScoreText = GameObject.Find("ScoreText").gameObject.GetComponent<Text>(); //Reference to component displaying user score
		m_ScoreText.text = "Score: " + m_Score;	// Sets the Text for the user score
	}

	void Start() {
		m_Cubes [m_ActiveCubeIndex].rend.enabled = true;	//Sets the visibility of the active cube to true
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() {
		m_TimeLeft -= Time.deltaTime;	//Subtracts the elapsed time from the remaining time


		if (m_TimeLeft <= 0) {			//If time has expired call game over condition
			StartCoroutine(ResetMiniGame()); // Calls routine to reset the level.
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

	/************* CONSIDER CHANGING SO USER CLICKS TO DETERMINE WHEN TO RESET ******************/
	private IEnumerator ResetMiniGame()
	{
		m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", 0.0f); // Display remaining time as 0.00 as opposed to 0.01 etc
		yield return new WaitForSeconds( 2.0f ); //Delays game play
		m_Score = 0;							 //Resets user score to 0
		m_TimeLeft = 5.0f;						 //Reset remaining time to 5 secs
		m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", m_TimeLeft); // display 5 secs on clock
	}
}

