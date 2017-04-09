using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class CarList
{

	public List<CarController> _cars = new List<CarController>();

	public CarList ()
	{
	}

	public void AddCar(CarController car){
		_cars.Add (car);
	}

	public int GetNumberAliveCars(){
		int aliveCars = 0;
		foreach (CarController car in _cars) {
			if (car.Alive) {
				aliveCars++;
			}
		}
		return aliveCars;
	}

	public void ClearAllDisconnectedPlayers(){
		for(var i = _cars.Count - 1; i > -1; i--)
		{
			UnityEngine.Debug.Log ("car.connection: " + _cars [i].connectionToClient);
            if (_cars[i].connectionToClient == null)
            {
                _cars.RemoveAt(i);
            }
		}
	}

	public int GetNumberOfBombsPresent(){
		int bombs = 0;
		foreach (CarController car in _cars) {
			if (car.HasBomb && car.Alive) {
				bombs++;
			}
		}
		return bombs;
	}

	private string GetPlayerName (CarController car) {
		return string.Format ("player {0}", car.connectionToClient.connectionId);
	}

	public void PassBombRandomPlayer(){
        //We should only call this function when there are no bombs 
        if (GetNumberOfBombsPresent() != 0)
        {
            UnityEngine.Debug.LogError("There is already at least one car with a bomb.");
            return;
        }
		foreach (CarController car in _cars) {
			if (car.Alive && !car.HasBomb) {
				car.setBombAllDevices (true);
				return;
			}
		}
	}

	public List<CarController> GetCarsOutOfTime(){
		List<CarController> outOfTimeCars = new List<CarController> ();
		foreach (CarController car in _cars) {
			if (car.Lifetime < 0.0f) {
				outOfTimeCars.Add (car);
			}
		}
		return outOfTimeCars;
	}

	public void TickTime(float time){
		foreach (CarController car in _cars) {
			if (car.HasBomb && car.Lifetime > 0.0f) {
				car.Lifetime -= time;
			}
		}
	}

    private float GetSurvivalTime(CarController car)
    {
        //Get total number of cars
        float total = _cars.Count;
        //Find total game time
        float total_time = total * car.MaxLifetime;

        float life_sum = 0.0f;
        foreach (CarController c in _cars)
        {
            life_sum += c.Lifetime;
        }
        return total_time - life_sum + car.Lifetime;
    }

	public void KillPlayer (CarController car) {
		car.KillAllDevices ();
		int carsLeft = GetNumberAliveCars();
		ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, carsLeft, GetSurvivalTime(car));
	}

	public void FinaliseGamePlayerData(){
		foreach (CarController car in _cars) {
			if (car.Alive) {
				ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, 0, GetSurvivalTime(car));
				return;
			}
		}
	}

	public void enableAllControls(){
		foreach (CarController car in _cars) {
			car.RpcEnableControls ();
		}
	}
		

	public void StartGameCountDown (){
		foreach (CarController car in _cars) {
			car.RpcStartGameCountDown ();
		}
	}

}

