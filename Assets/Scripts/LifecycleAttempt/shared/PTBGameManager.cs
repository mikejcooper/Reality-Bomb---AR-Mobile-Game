using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkBehaviour {

	public List<CarController> m_Cars = new List<CarController>();

	public List<CarController> m_DeathOrder = new List<CarController>();

	public int bombPlayerIndex;
	public int bombPlayerConnectionId;

	void Start () {
		if (isServer) {
			// index 1 is server I think
			bombPlayerIndex = Random.Range(1, UnityEngine.Networking.NetworkServer.connections.Count);
			DebugConsole.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, UnityEngine.Networking.NetworkServer.connections.Count));
			bombPlayerConnectionId = UnityEngine.Networking.NetworkServer.connections [bombPlayerIndex].connectionId;
			DebugConsole.Log ("=> bombPlayerConnectionId: " + bombPlayerConnectionId);
		}
	}

	private void Update()
	{
		CheckDeaths();
		if (GameOver ()) {
			DebugConsole.Log ("GAME OVER");
			GameObject.Find ("Persistent").GetComponent<ServerSceneManager> ().onGameEnd (makeGameResults());
		}

	}

	private GameResults makeGameResults () {
		GameResults results = new GameResults ();
		results.deaths = m_DeathOrder;
		return results;
	}

	public void AddCar(GameObject gamePlayer)
	{
		m_Cars.Add(gamePlayer.GetComponent<CarController>());
		Debug.Log("Car added!");
	}

	private void CheckDeaths()
	{
		for (int i = 0; i < m_Cars.Count; i++)
		{
			if (!m_Cars[i].alive)
			{
				m_DeathOrder.Add(m_Cars[i]);
				m_Cars.RemoveAt(i);
			}
		}
	}

	private bool GameOver()
	{
		return m_Cars.Count == 1;
	}

//	public void RepositionAllCars()
//	{
//		for (int i = 0; i < m_Cars.Count; i++)
//		{
//			m_Cars[i].Reposition();
//		}
//		EnableCameraLayer();
//	}

//	void EnableCameraLayer()
//	{
//		Camera.current.cullingMask |= 1 << LayerMask.NameToLayer("Players");
//	}
}