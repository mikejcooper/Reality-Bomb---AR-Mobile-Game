using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameLobbyManager : NetworkLobbyManager {
	private int players = 0;
	private int bombPlayer = -1;

	public delegate void OnLobbyServerConnectCallback ();
	public delegate void OnLobbyServerDisconnectCallback ();
	public delegate void OnLobbyClientConnectCallback ();
	public delegate void OnLobbyClientDisconnectCallback ();

	public event OnLobbyServerConnectCallback OnLobbyServerConnectEvent;
	public event OnLobbyServerDisconnectCallback OnLobbyServerDisconnectEvent;
	public event OnLobbyClientConnectCallback OnLobbyClientConnectEvent;
	public event OnLobbyClientDisconnectCallback OnLobbyClientDisconnectEvent;

	public override void OnLobbyServerPlayersReady () {
		DebugConsole.Log ("OnLobbyServerPlayersReady");
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
	{
		if (bombPlayer == -1)
		{
			bombPlayer = Random.Range(0, numPlayers);
		}
		var gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
		if (players == bombPlayer)
		{
			gamePlayer.gameObject.GetComponent<CarController>().hasBomb = true;
		}
		players++;
		return gamePlayer;
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		PTBGameManager.AddCar(gamePlayer);
		return true;
	}

	public override void OnLobbyServerConnect (NetworkConnection conn) {
		OnLobbyServerConnectEvent ();
	}

	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		OnLobbyServerDisconnectEvent ();
	}

	public override void OnLobbyClientConnect (NetworkConnection conn) {
		OnLobbyClientConnectEvent ();
	}

	public override void OnLobbyClientDisconnect (NetworkConnection conn) {
		OnLobbyClientDisconnectEvent ();
	}
}