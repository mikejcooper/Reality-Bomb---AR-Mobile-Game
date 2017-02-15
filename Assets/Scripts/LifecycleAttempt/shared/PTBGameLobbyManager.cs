using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameLobbyManager : NetworkCompat.NetworkLobbyManager {

	public delegate void OnLobbyServerConnectCallback (NetworkConnection conn);
	public delegate void OnLobbyServerDisconnectCallback ();
	public delegate void OnLobbyClientConnectCallback ();
	public delegate void OnLobbyClientDisconnectCallback ();

	public event OnLobbyServerConnectCallback OnLobbyServerConnectEvent;
	public event OnLobbyServerDisconnectCallback OnLobbyServerDisconnectEvent;
	public event OnLobbyClientConnectCallback OnLobbyClientConnectEvent;
	public event OnLobbyClientDisconnectCallback OnLobbyClientDisconnectEvent;

//	public override void OnLobbyServerPlayersReady () {
//		DebugConsole.Log ("OnLobbyServerPlayersReady");
//	}

//	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
//	{
//		NetworkServer.connections
//		return null;
//	}

//	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayerGameObject, GameObject gamePlayer) {
//		
//	}

	public void ServerChangeSceneForceLobby(string sceneName)
	{
		
		foreach (var lobbyPlayer in lobbySlots)
		{
			if (lobbyPlayer == null)
				continue;

			// find the game-player object for this connection, and destroy it
			var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

			UnityEngine.Networking.PlayerController playerController = NetworkCompat.Utils.GetPlayerController (uv.playerControllerId, uv.connectionToClient);
			if (playerController != null)
			{
				NetworkServer.Destroy(playerController.gameObject);
			}

			if (NetworkServer.active)
			{
				// re-add the lobby object
				lobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
				NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, lobbyPlayer.gameObject, uv.playerControllerId);
			}
		}

		base.ServerChangeScene(sceneName);
	}


	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		
		DebugConsole.Log ("OnServerAddPlayer: "+playerControllerId);
		base.OnServerAddPlayer (conn, playerControllerId);


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

//	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
//	{
//		NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, gamePlayer);
//		return false;
//	}

	public override void OnLobbyServerConnect (NetworkConnection conn) {
		OnLobbyServerConnectEvent (conn);

	}



	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		OnLobbyServerDisconnectEvent ();
	}

	public override void OnLobbyServerPlayerRemoved(UnityEngine.Networking.NetworkConnection conn, short playerControllerId) {
		DebugConsole.Log ("OnLobbyServerPlayerRemoved");
		Debug.Log ("OnLobbyServerPlayerRemoved");
	}

	public override void OnLobbyServerPlayersReady() {
		DebugConsole.Log ("OnLobbyServerPlayersReady");
		Debug.Log ("OnLobbyServerPlayersReady");
	}

	public override GameObject OnLobbyServerCreateGamePlayer(UnityEngine.Networking.NetworkConnection conn, short playerControllerId) {
		DebugConsole.Log ("OnLobbyServerCreateGamePlayer");
		Debug.Log ("OnLobbyServerCreateGamePlayer");
		return null;

	}

	public override GameObject OnLobbyServerCreateLobbyPlayer(UnityEngine.Networking.NetworkConnection conn, short playerControllerId) {
		DebugConsole.Log ("OnLobbyServerCreateLobbyPlayer");
		Debug.Log ("OnLobbyServerCreateLobbyPlayer");
		return null;
	}

	public override void OnLobbyClientConnect (NetworkConnection conn) {
		DebugConsole.Log ("OnLobbyClientConnect");
		Debug.Log ("OnLobbyClientConnect");
	}

	public override void OnLobbyClientDisconnect (NetworkConnection conn) {
		OnLobbyClientDisconnectEvent ();
	}

	public override void OnClientError (NetworkConnection conn, int err) {
		DebugConsole.Log ("error: " + err);
	}

	public override void OnStartClient(NetworkClient lobbyClient)
	{
		

		if (lobbySlots.Length == 0)
		{
			lobbySlots = new NetworkCompat.NetworkLobbyPlayer[maxPlayers];
		}

		base.OnStartClient (lobbyClient);
	}

	public override void OnLobbyClientAddPlayerFailed () {
		Debug.Log ("OnLobbyClientAddPlayerFailed");
	}

	public override void OnLobbyClientEnter () {
		Debug.Log ("OnLobbyClientEnter");
		OnLobbyClientConnectEvent ();
	}

	public override void OnLobbyClientExit () {
		Debug.Log ("OnLobbyClientExit");
	}

	public override void OnLobbyClientSceneChanged(UnityEngine.Networking.NetworkConnection conn) {
		Debug.Log ("OnLobbyClientSceneChanged");
	}

//	public override void OnClientConnect (UnityEngine.Networking.NetworkConnection conn) {
//		DebugConsole.Log ("OnClientConnect");
//		if (client != null) {
//			DebugConsole.Log ("OnClientConnect worked");
//			client.RegisterHandler (928, OnClientMeshDataReceived);		
//		}
//	}
//
//	public void OnClientMeshDataReceived (NetworkMessage netMsg) {
//		DebugConsole.Log ("OnClientMeshDataReceived");
//		string meshData = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.StringMessage>().value;
//		Debug.Log (meshData);
//	}
		
}