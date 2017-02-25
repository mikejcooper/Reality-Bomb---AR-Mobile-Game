using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GlobalNetworking;
using NetworkCompat;

public class GameLobbyManager : NetworkCompat.NetworkLobbyManager {

	public delegate void OnLobbyServerConnected (NetworkConnection conn);
	public delegate void OnLobbyServerDisconnected ();
	public delegate void OnLobbyClientConnected ();
	public delegate void OnLobbyClientDisconnected ();
	public delegate void OnLobbyClientReadyToBegin ();
	public delegate void OnLobbyClientGameLoaded ();
    public delegate void OnMeshClearToDownloadCallback(string address, int port);

    public event OnLobbyServerConnected OnLobbyServerConnectedEvent;
	public event OnLobbyServerDisconnected OnLobbyServerDisconnectedEvent;
	public event OnLobbyClientConnected OnLobbyClientConnectedEvent;
	public event OnLobbyClientDisconnected OnLobbyClientDisconnectedEvent;
	public event OnLobbyClientReadyToBegin OnLobbyClientReadyToBeginEvent;
	public event OnLobbyClientGameLoaded OnLobbyClientGameLoadedEvent;
    public event OnMeshClearToDownloadCallback OnMeshClearToDownloadEvent;

	public override void OnLobbyServerGameLoaded(NetworkConnection conn) {
		if (OnLobbyClientGameLoadedEvent != null)
			OnLobbyClientGameLoadedEvent ();
	}

	// sent from server to set all its clients to not ready and tell all clients they are not ready
	public void SetAllClientsNotReady () {
		foreach (var player in lobbySlots)
		{
			if (player != null)
			{
				player.GetComponent<NetworkCompat.NetworkLobbyPlayer>().readyToBegin = false;

				LobbyReadyToBeginMessage msg = new LobbyReadyToBeginMessage ();
				msg.slotId = player.slot;
				msg.readyState = false;
				UnityEngine.Networking.NetworkServer.SendToReady(null, UnityEngine.Networking.MsgType.LobbyReadyToBegin, msg);
			}
		}
	}

	// sent from a client to tell the server it's ready
	public void SetReady () {
		// notify server that we're ready
		foreach (var p in lobbySlots) {
			if (p != null && p.playerControllerId >= 0) {
				var msg = new LobbyReadyToBeginMessage ();
				msg.slotId = (byte)p.playerControllerId;
				msg.readyState = true;
				client.Send (UnityEngine.Networking.MsgType.LobbyReadyToBegin, msg);
			}
		}
	}

	// sent from a client to tell the server the game has loaded
	public void SetGameLoaded () {
		// notify server that we're ready
		foreach (var p in lobbySlots) {
			if (p != null && p.playerControllerId >= 0) {
				var msg = new GameLoadedMessage ();
				msg.slotId = (byte)p.playerControllerId;
				msg.loadedState = true;
				client.Send (NetworkConstants.MSG_GAME_LOADED, msg);
			}
		}
	}

	// sent from server to all clients to tell them to download a mesh
	public void AllClientsGetMesh (string address, int port) {
		ServerNetworking.SocketMessage msg = new ServerNetworking.SocketMessage();
		msg.address = address;
		msg.port = port;
		UnityEngine.Networking.NetworkServer.SendToAll(NetworkConstants.MSG_GET_MESH, msg);
	}

	// sent from server to specific client to tell it to download a mesh
	public void ClientGetMesh(string address, int port, int connectionId) {
		ServerNetworking.SocketMessage msg = new ServerNetworking.SocketMessage ();
		msg.address = address;
		msg.port = port;
		UnityEngine.Networking.NetworkServer.SendToClient (connectionId, NetworkConstants.MSG_GET_MESH, msg);
	}

	public override void OnLobbyServerConnect (NetworkConnection conn) {
		OnLobbyServerConnectedEvent (conn);

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

	public override void OnLobbyServerReadyToBegin(NetworkConnection conn) {
		if (OnLobbyClientReadyToBeginEvent != null)
			OnLobbyClientReadyToBeginEvent ();
	}


	public override void OnLobbyClientEnter () {
		Debug.Log ("OnLobbyClientEnter");
		OnLobbyClientConnectedEvent ();
	}

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        if (client != null)
        {
            DebugConsole.Log("OnClientConnect worked");
			client.RegisterHandler(NetworkConstants.MSG_GET_MESH, OnClientClearToDownloadMesh);
			client.RegisterHandler(NetworkConstants.MSG_GAME_LOADED, OnClientGameReady);
<<<<<<< HEAD
        }
=======
		}
>>>>>>> c24dd28ae605a2c41613834e75f30ea324d882a7
    }

	public void OnClientGameReady(NetworkMessage netMsg) {
		Debug.Log ("OnClientGameReady");
	}

    public void OnClientClearToDownloadMesh(NetworkMessage netMsg) {
        ServerNetworking.SocketMessage socketMsg = netMsg.ReadMessage<ServerNetworking.SocketMessage>();
        Debug.Log(socketMsg.address);

        OnMeshClearToDownloadEvent(socketMsg.address, socketMsg.port);
    }
}