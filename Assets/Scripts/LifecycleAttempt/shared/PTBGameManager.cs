using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkBehaviour {

	public static PTBGameManager s_Instance;

	public static List<CarController> m_Cars = new List<CarController>();

	public static List<CarController> m_DeathOrder = new List<CarController>();

	private void Awake(){
		s_Instance = this;
	}

	private void Update()
	{
		CheckDeaths();
		if (GameOver())
			Debug.Log("GAME OVER");
	}

	public static void AddCar(GameObject gamePlayer)
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

	public void RepositionAllCars()
	{
		for (int i = 0; i < m_Cars.Count; i++)
		{
			m_Cars[i].Reposition();
		}
		EnableCameraLayer();
	}

	void EnableCameraLayer()
	{
		Camera.current.cullingMask |= 1 << LayerMask.NameToLayer("Players");
	}
}