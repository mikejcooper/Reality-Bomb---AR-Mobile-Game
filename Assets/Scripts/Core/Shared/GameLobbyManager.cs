﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameLobbyManager : NetworkCompat.NetworkLobbyManager {

	public delegate void OnLobbyServerConnected ();
	public delegate void OnLobbyServerDisconnected ();
	public delegate void OnLobbyClientConnected ();
	public delegate void OnLobbyClientDisconnected ();

	public event OnLobbyServerConnected OnLobbyServerConnectedEvent;
	public event OnLobbyServerDisconnected OnLobbyServerDisconnectedEvent;
	public event OnLobbyClientConnected OnLobbyClientConnectedEvent;
	public event OnLobbyClientDisconnected OnLobbyClientDisconnectedEvent;


	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		
		DebugConsole.Log ("OnServerAddPlayer: "+playerControllerId);
		base.OnServerAddPlayer (conn, playerControllerId);

		// automatically set them as ready - client has no choice
		UnityEngine.Networking.PlayerController lobbyController = null;
		for (int i = 0; i < conn.playerControllers.Count; i++) {
			if (conn.playerControllers [i] != null && conn.playerControllers [i].playerControllerId == playerControllerId) {
				lobbyController = conn.playerControllers [i];
				break;
			}
		}
		if (lobbyController == null)
		{
			Debug.LogError ("NetworkLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + playerControllerId);
			return;
		}

		// set this player ready
		var lobbyPlayer = lobbyController.gameObject.GetComponent<NetworkLobbyPlayer>();
		lobbyPlayer.readyToBegin = true;

		// tell every player that this player is ready
		var outMsg = new NetworkCompat.LobbyReadyToBeginMessage();
		outMsg.slotId = lobbyPlayer.slot;
		outMsg.readyState = true;
		NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

	}

	public override void OnLobbyServerConnect (NetworkConnection conn) {
		OnLobbyServerConnectedEvent ();

	}

	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		OnLobbyServerDisconnectedEvent ();
	}

	public override void OnLobbyClientDisconnect (NetworkConnection conn) {
		OnLobbyClientDisconnectedEvent ();
	}

	public override void OnClientError (NetworkConnection conn, int err) {
		DebugConsole.Log ("error: " + err);
	}


	public override void OnLobbyClientEnter () {
		Debug.Log ("OnLobbyClientEnter");
		OnLobbyClientConnectedEvent ();
	}
}