using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameManager : NetworkBehaviour {

    public static PTBGameManager s_Instance;

    public static List<CarController> m_Players = new List<CarController>();

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
        m_Players.Add(gamePlayer.GetComponent<CarController>());
        Debug.Log("Tank added!");
    }

    private void CheckDeaths()
    {
        for (int i = 0; i < m_Players.Count; i++)
        {
            if (!m_Players[i].alive)
            {
                m_DeathOrder.Add(m_Players[i]);
                m_Players.RemoveAt(i);
            }
        }
    }

    private bool GameOver()
    {
        return m_Players.Count == 1;
    }

    public void RepositionAllCars()
    {
        if (!isServer)
            return;
        for (int i = 0; i < m_Players.Count; i++)
        {
            m_Players[i].Reposition();
        }
        EnableCameraLayer();
    }

    void EnableCameraLayer()
    {
        Camera.current.cullingMask |= 1 << LayerMask.NameToLayer("Players");
    }
}
