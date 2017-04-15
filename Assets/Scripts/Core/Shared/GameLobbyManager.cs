using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GlobalNetworking;
using NetworkCompat;

public class GameLobbyManager : NetworkCompat.NetworkLobbyManager {

	public delegate void OnLobbyServerConnected (NetworkConnection conn);
	public delegate void OnLobbyServerDisconnected (NetworkConnection conn);
	public delegate void OnLobbyClientConnected ();
	public delegate void OnLobbyClientDisconnected ();
	public delegate void OnLobbyClientReadyToBegin ();
	public delegate void OnLobbyClientGameLoaded ();
	public delegate void OnLobbyClientNameSubmission (int connId, string name);
    public delegate void OnMeshClearToDownloadCallback(string address, int port);
	public delegate void OnUpdatePlayerDataCallback (string data);
	public delegate void OnPlayerIDCallback (int playerID);
	public delegate void OnStartGameCountdownCallback (int delay);
	public delegate void OnCancelGameCountdownCallback (string reason);

    public event OnLobbyServerConnected OnLobbyServerConnectedEvent;
	public event OnLobbyServerDisconnected OnLobbyServerDisconnectedEvent;
	public event OnLobbyClientConnected OnLobbyClientConnectedEvent;
	public event OnLobbyClientDisconnected OnLobbyClientDisconnectedEvent;
	public event OnLobbyClientReadyToBegin OnLobbyClientReadyToBeginEvent;
	public event OnLobbyClientGameLoaded OnLobbyAllClientGamesLoadedEvent;
	public event OnLobbyClientNameSubmission OnLobbyClientNameSubmissionEvent;
    public event OnMeshClearToDownloadCallback OnMeshClearToDownloadEvent;
	public event OnUpdatePlayerDataCallback OnUpdatePlayerDataEvent;
	public event OnPlayerIDCallback OnPlayerIDEvent;
	public event OnStartGameCountdownCallback OnStartGameCountdownEvent;
	public event OnCancelGameCountdownCallback OnCancelGameCountdownEvent;

    private int _totalNotLoaded = -10;

	public override void OnLobbyStartServer() {
		NetworkServer.RegisterHandler(NetworkConstants.MSG_PLAYER_DATA_SUBMIT, OnServerPlayerDataSubmission);
	}

	public void OnServerPlayerDataSubmission (NetworkMessage netMsg) {
		string name = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.StringMessage> ().value;
		if (OnLobbyClientNameSubmissionEvent != null) {
			OnLobbyClientNameSubmissionEvent (netMsg.conn.connectionId, name);
		}
	}

	public override void OnLobbyServerGameLoaded(NetworkConnection conn) {
        if (_totalNotLoaded == -10)
        {
            _totalNotLoaded = NetworkServer.connections.Count - 1; //Minus 1 because this includes the server
        }
        _totalNotLoaded--;

        if (_totalNotLoaded == 0)
        {
            //Trigger an event to start the countdown on all players
            if (OnLobbyAllClientGamesLoadedEvent != null)
                OnLobbyAllClientGamesLoadedEvent(); //Calls OnStateUpdate
            _totalNotLoaded = -10; //Reset
        }
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
		var obj = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
		obj.GetComponent<CarController> ().ServerId = conn.connectionId;
		return obj;
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
				UnityEngine.Networking.NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, msg);
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
		NetworkServer.SendToAll(NetworkConstants.MSG_GET_MESH, msg);
	}

	// sent from server to specific client to tell it to download a mesh
	public void ClientGetMesh(string address, int port, int connectionId) {
		ServerNetworking.SocketMessage msg = new ServerNetworking.SocketMessage ();
		msg.address = address;
		msg.port = port;
		NetworkServer.SendToClient (connectionId, NetworkConstants.MSG_GET_MESH, msg);
	}

	public void AllClientsStartGameCountdown (int delay) {
		ServerNetworking.StartGameCountdownMessage msg = new ServerNetworking.StartGameCountdownMessage();
		msg.delay = delay;
		NetworkServer.SendToAll(NetworkConstants.MSG_START_GAME_COUNTDOWN, msg);
	}

	public void AllClientsCancelGameCountdown (string reason) {
		ServerNetworking.CancelGameCountdownMessage msg = new ServerNetworking.CancelGameCountdownMessage();
		msg.reason = reason;
		NetworkServer.SendToAll(NetworkConstants.MSG_CANCEL_GAME_COUNTDOWN, msg);
	}

	public void SendPlayerData (string data, int connectionId) {
		var msg = new UnityEngine.Networking.NetworkSystem.StringMessage (data);

		NetworkServer.SendToClient (connectionId, NetworkConstants.MSG_PLAYER_DATA_UPDATE, msg);
	}

	public void SendPlayerID (int connectionId) {
		var msg = new UnityEngine.Networking.NetworkSystem.IntegerMessage (connectionId);
		NetworkServer.SendToClient (connectionId, NetworkConstants.MSG_PLAYER_DATA_ID, msg);
	}

	public void UpdatePlayerData (string data) {
		foreach (var p in lobbySlots) {
			if (p != null && p.playerControllerId >= 0) {
				var msg = new UnityEngine.Networking.NetworkSystem.StringMessage (data);
				NetworkServer.SendToAll (NetworkConstants.MSG_PLAYER_DATA_UPDATE, msg);
			}
		}
	}

	public void SendOwnPlayerName (string name) {
		var msg = new UnityEngine.Networking.NetworkSystem.StringMessage (name);

		client.Send (NetworkConstants.MSG_PLAYER_DATA_SUBMIT, msg);
	}

	public override void OnLobbyServerConnect (NetworkConnection conn) {
		OnLobbyServerConnectedEvent (conn);
	}

	public override void OnLobbyServerDisconnect (NetworkConnection conn) {
		OnLobbyServerDisconnectedEvent (conn);
	}

	public override void OnLobbyClientDisconnect (NetworkConnection conn) {
		OnLobbyClientDisconnectedEvent ();
	}

	public override void OnClientError (NetworkConnection conn, int err) {
		Debug.Log ("error: " + err);
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
            Debug.Log("OnClientConnect worked");
			client.RegisterHandler(NetworkConstants.MSG_GET_MESH, OnClientClearToDownloadMesh);
			client.RegisterHandler(NetworkConstants.MSG_GAME_LOADED, OnClientGameReady);
			client.RegisterHandler(NetworkConstants.MSG_PLAYER_DATA_UPDATE, OnClientPlayerDataUpdate);
			client.RegisterHandler(NetworkConstants.MSG_PLAYER_DATA_ID, OnClientPlayerID);
			client.RegisterHandler(NetworkConstants.MSG_START_GAME_COUNTDOWN, OnStartGameCountdown);
			client.RegisterHandler(NetworkConstants.MSG_CANCEL_GAME_COUNTDOWN, OnCancelGameCountdown);
		}
    }

	public void OnStartGameCountdown (NetworkMessage netMsg) {
		ServerNetworking.StartGameCountdownMessage msg = netMsg.ReadMessage<ServerNetworking.StartGameCountdownMessage> ();
		if (OnStartGameCountdownEvent != null) {
			OnStartGameCountdownEvent (msg.delay);
		}
	}

	public void OnCancelGameCountdown (NetworkMessage netMsg) {
		ServerNetworking.CancelGameCountdownMessage msg = netMsg.ReadMessage<ServerNetworking.CancelGameCountdownMessage> ();
		if (OnCancelGameCountdownEvent != null) {
			OnCancelGameCountdownEvent (msg.reason);
		}
	}

	public void OnClientPlayerID (NetworkMessage netMsg) {
		if (OnPlayerIDEvent != null) {
			OnPlayerIDEvent(netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.IntegerMessage>().value);
		}
	}

	public void OnClientPlayerDataUpdate (NetworkMessage netMsg) {
		if (OnUpdatePlayerDataEvent != null) {
			OnUpdatePlayerDataEvent(netMsg.reader.ReadString());
		}
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