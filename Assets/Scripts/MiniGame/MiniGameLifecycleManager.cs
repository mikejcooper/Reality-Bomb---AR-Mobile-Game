using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameLifecycleManager : MonoBehaviour
{
	public GameObject m_SpawnPoint;
	public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
	public TankManager m_Tank;		//Reference the to tank object

	public CubeCollisionManager m_Cube;		//Reference to the Cube in the scene
	private float m_TimeLeft;		//Remaining time to get to Trigger Zone
	private Text m_TimeLeftText;	//Text displaying the amount of time left
	private int m_Score;			//Keeps count of the score
	private Text m_ScoreText;		//Text displaying the current score
	private bool m_GameOver;		//Used to check if game is currently being played

	private float m_StartTime;		//The time at which the game will be reset


	void Awake() 
	{
		m_TimeLeft = 5.0f;			//Initialises time on clock to 5 secs
		m_TimeLeftText = GameObject.Find("TimeLeftText").gameObject.GetComponent<Text>();  //Reference to Component which displays tiem remaining
		m_TimeLeftText.text = "Time Left: " + string.Format("{0:N2}", m_TimeLeft); //Sets the time remaining text
		m_Score = 0;				//Initialises user score to 0
		m_ScoreText = GameObject.Find("ScoreText").gameObject.GetComponent<Text>(); //Reference to component displaying user score
		m_ScoreText.text = "Score: " + m_Score;	// Sets the Text for the user score
		m_StartTime = Time.time;	//Get current time
		m_GameOver = false;			//Initially game such that we haven't run out of time
	}

	void Start() 
	{
		m_Cube.rend.enabled = true;	//Sets the visibility of the active cube to true
		m_Tank.m_SpawnPoint = m_SpawnPoint.transform.position;
		Debug.Log("starting");
		SpawnTank();
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() 
	{
		if (m_Tank.m_Instance.activeSelf && false) {
			m_TimeLeft -= Time.deltaTime;	//Subtracts the elapsed time from the remaining time

			//If the player has run out of time
			if (m_TimeLeft < 0) {	
				//If we have just lost then set then reset the game and set GameOver to true
				if (m_GameOver == false) {
					m_GameOver = true;		
					m_StartTime = Time.time + 3.0f;
					m_TimeLeftText.text = "Time Left: " + string.Format ("{0:N2}", 5.0f);
					m_Score = 0;
					m_ScoreText.text = "Score: " + m_Score; //Updates the current user score
					m_Tank.DisableControl ();
					m_Tank.Reset ();
				}
				//wait until the game restarts and then set Gameover to false begin playing again
				if (Time.time > m_StartTime) {
					m_TimeLeft = 5.0f;
					m_GameOver = false;
					m_Tank.EnableControl ();
				}
			} 
		//If the player still has time left
		else {			
				m_TimeLeftText.text = "Time Left: " + string.Format ("{0:N2}", m_TimeLeft); //Updates the remaining time
				m_ScoreText.text = "Score: " + m_Score; //Updates the current user score

				//if active cube is entered
				if (m_Cube.IsTriggered () == true) {	//If player has entered trigger zone
					//update active cube
					UpdateCube ();			//Move the cube to a new random position
					m_TimeLeft += 5.0f;		//Increment time left by 5 secs
					m_Score++;				//Increment user score
				}
			}
		}
	}

	//Give the cube a new random position
	private void UpdateCube() 
	{
		m_Cube.transform.position = new Vector3 (Random.Range (-20.0f,20.0f), 0, Random.Range(-20.0f,20.0f));
	}

	//Create the tank object and initialise its values
	private void SpawnTank()
	{
	
		m_Tank.m_Instance = Instantiate(m_TankPrefab, m_Tank.m_SpawnPoint, Quaternion.Euler(new Vector3(0,0,0))) as GameObject;
		m_Tank.m_PlayerNumber = 0;
		m_Tank.m_Instance.GetComponent<TankController>().isPlayingSolo = true;
		m_Tank.Setup();

		// attach to same object as spawn point
		m_Tank.m_Instance.transform.parent = m_SpawnPoint.transform.parent.transform;

		// set visibility based on whether or not the marker is currently visible
//		GameObject toolkit = GameObject.Find("ARToolKit");
//		ARMarker marker = toolkit.GetComponent<ARMarker>();
//		m_Tank.m_Instance.SetActive(marker.Visible);

	}


}

