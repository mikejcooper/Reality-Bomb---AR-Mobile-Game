using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameLifecycleManager : MonoBehaviour
{
	public GameObject SpawnPoint;
	// Reference to the prefab the players will control.
	public GameObject CarPrefab;
	// Reference the to MiniGameController object
	public MiniGameController MiniGameController;		
	// Reference to the Cube in the scene
	public CubeCollisionManager CubeCollisionManager;

	// Remaining time to get to Trigger Zone
	private float _timeLeft;
	// Text displaying the amount of time left
	private Text _timeLeftText;
	// Keeps count of the score
	private int _score;
	// Text displaying the current score
	private Text _scoreText;
	// Used to check if game is currently being played
	private bool _gameOver;
	// The time at which the game will be reset
	private float _startTime;


	void Awake() 
	{
		// Initialises time on clock to 5 secs
		_timeLeft = 5.0f;
		// Reference to Component which displays tiem remaining
		_timeLeftText = GameObject.Find("TimeLeftText").gameObject.GetComponent<Text>();
		// Sets the time remaining text
		_timeLeftText.text = "Time Left: " + string.Format("{0:N2}", _timeLeft);
		// Initialises user score to 0
		_score = 0;
		// Reference to component displaying user score
		_scoreText = GameObject.Find("ScoreText").gameObject.GetComponent<Text>();
		// Sets the Text for the user score
		_scoreText.text = "Score: " + _score;
		// Get current time
		_startTime = Time.time;
		// Initially game such that we haven't run out of time
		_gameOver = false;			
	}

	void Start() 
	{
		// Sets the visibility of the active cube to true
		CubeCollisionManager.Renderer.enabled = true;
		MiniGameController.SpawnPoint = SpawnPoint.transform.position;
		SpawnCar();
	}


	// This is called from start and will run each phase of the game one after another.
	void Update() 
	{
		if (MiniGameController.CarObject.activeSelf) {
			// Subtracts the elapsed time from the remaining time
			_timeLeft -= Time.deltaTime;

			// If the player has run out of time
			if (_timeLeft < 0) {	
				// If we have just lost then set then reset the game and set GameOver to true
				if (_gameOver == false) {
					_gameOver = true;		
					_startTime = Time.time + 3.0f;
					_timeLeftText.text = "Time Left: " + string.Format ("{0:N2}", 5.0f);
					_score = 0;
					// Updates the current user score
					_scoreText.text = "Score: " + _score; 
					MiniGameController.DisableControl ();
					MiniGameController.Reset ();
				}
				// wait until the game restarts and then set Gameover to false begin playing again
				if (Time.time > _startTime) {
					_timeLeft = 5.0f;
					_gameOver = false;
					MiniGameController.EnableControl ();
				}
			} else {	
				// If the player still has time left
				// Updates the remaining time
				_timeLeftText.text = "Time Left: " + string.Format ("{0:N2}", _timeLeft);
				// Updates the current user score
				_scoreText.text = "Score: " + _score;

				// if active cube is entered
				if (CubeCollisionManager.IsTriggered () == true) {	
					// If player has entered trigger zone
					// update active cube
					// Move the cube to a new random position
					UpdateCube ();
					// Increment time left by 5 secs
					_timeLeft += 5.0f;
					// Increment user score
					_score++;
				}
			}
		}
	}

	// Give the cube a new random position
	private void UpdateCube() 
	{
		CubeCollisionManager.transform.position = new Vector3 (Random.Range (-100.0f,100.0f), 6, Random.Range(-100.0f,100.0f));
	}

	// Create the car object and initialise its values
	private void SpawnCar()
	{
	
		MiniGameController.CarObject = Instantiate(CarPrefab, MiniGameController.SpawnPoint, Quaternion.Euler(new Vector3(0,0,0))) as GameObject;
		MiniGameController.CarObject.GetComponent<OfflineCarController>().IsPlayingSolo = true;
		MiniGameController.Setup();

		// attach to same object as spawn point
		MiniGameController.CarObject.transform.parent = SpawnPoint.transform.parent.transform;

	}

}

