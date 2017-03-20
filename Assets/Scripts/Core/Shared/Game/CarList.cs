﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class CarList
{

	private List<CarController> _cars = new List<CarController>();

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
			if (_cars[i].connectionToClient == null) _cars.RemoveAt(i);
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

	public void KillPlayer (CarController car) {
		car.KillAllDevices ();
		int carsLeft = GetNumberAliveCars();
		ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, carsLeft, car.Lifetime);
	}

	public void FinaliseGamePlayerData(){
		foreach (CarController car in _cars) {
			if (car.Alive) {
				ServerSceneManager.Instance.UpdatePlayerGameData (car.ServerId, 0, car.Lifetime);
				return;
			}
		}
	}

}

