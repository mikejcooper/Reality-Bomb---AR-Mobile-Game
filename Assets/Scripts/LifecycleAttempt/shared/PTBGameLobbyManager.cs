using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PTBGameLobbyManager : NetworkLobbyManager {

	public delegate void OnLobbyServerConnectCallback (NetworkConnection conn);
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

//	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
//	{
//		NetworkServer.connections
//		return null;
//	}

//	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayerGameObject, GameObject gamePlayer) {
//		
//	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		
		DebugConsole.Log ("OnServerAddPlayer");
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
		var outMsg = new LobbyReadyToBeginMessage();
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

	class LobbyReadyToBeginMessage : UnityEngine.Networking.MessageBase
	{
		public byte slotId;
		public bool readyState;

		public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
		{
			slotId = reader.ReadByte();
			readyState = reader.ReadBoolean();
		}

		public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
		{
			writer.Write(slotId);
			writer.Write(readyState);
		}
	}

	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		OnLobbyServerDisconnectEvent ();
	}

	public override void OnLobbyClientConnect (NetworkConnection conn) {
		
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
			lobbySlots = new NetworkLobbyPlayer[maxPlayers];
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