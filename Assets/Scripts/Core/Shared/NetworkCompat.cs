﻿using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace NetworkCompat {
	
	public class LobbyReadyToBeginMessage : MessageBase
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


	public class GameLoadedMessage : MessageBase
	{
		public byte slotId;
		public bool loadedState;

		public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
		{
			slotId = reader.ReadByte();
			loadedState = reader.ReadBoolean();
		}

		public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
		{
			writer.Write(slotId);
			writer.Write(loadedState);
		}

	}

	public class Utils {
		static public bool GetPlayerController(short id, NetworkConnection conn, out UnityEngine.Networking.PlayerController player) {
			for (int i = 0; i < conn.playerControllers.Count; i++) {
				if (conn.playerControllers [i] != null && conn.playerControllers [i].playerControllerId == id) {
					player = conn.playerControllers [i];
					return true;
				}
			}
			player = null;
			return false;
		}
	}

	[AddComponentMenu("Network/NetworkLobbyManager")]
	public class NetworkLobbyManager : NetworkManager
	{
		struct PendingPlayer
		{
			public NetworkConnection conn;
			public GameObject lobbyPlayer;
		}

		// configuration
		[SerializeField] bool m_ShowLobbyGUI = true;
		[SerializeField] int m_MaxPlayers = 4;
		[SerializeField] int m_MaxPlayersPerConnection = 1;
		[SerializeField] int m_MinPlayers;
		[SerializeField] NetworkLobbyPlayer m_LobbyPlayerPrefab;
		[SerializeField] GameObject m_GamePlayerPrefab;
		[SerializeField] string m_PlayScene = "";
		[SerializeField] List<string> m_LobbyScenes = new List<string>();

		private ColourPool _colourPool;

		// runtime data
		List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
		public NetworkLobbyPlayer[] lobbySlots;

		// static message objects to avoid runtime-allocations
		static LobbyReadyToBeginMessage s_ReadyToBeginMessage = new LobbyReadyToBeginMessage();
		static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
		static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();

		// properties
		public bool showLobbyGUI             { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
		public int maxPlayers                { get { return m_MaxPlayers; } set { m_MaxPlayers = value; } }
		public int maxPlayersPerConnection   { get { return m_MaxPlayersPerConnection; } set { m_MaxPlayersPerConnection = value; } }
		public int minPlayers                { get { return m_MinPlayers; } set { m_MinPlayers = value; } }
		public NetworkLobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
		public GameObject gamePlayerPrefab   { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
		public List<string> lobbyScenes             { get { return m_LobbyScenes; } set { m_LobbyScenes = value; offlineScene = value.First(); } }
		public string playScene              { get { return m_PlayScene; } set { m_PlayScene = value; } }

		void OnValidate()
		{
			if (m_MaxPlayers <= 0)
			{
				m_MaxPlayers = 1;
			}

			if (m_MaxPlayersPerConnection <= 0)
			{
				m_MaxPlayersPerConnection = 1;
			}

			if (m_MaxPlayersPerConnection > maxPlayers)
			{
				m_MaxPlayersPerConnection = maxPlayers;
			}

			if (m_MinPlayers < 0)
			{
				m_MinPlayers = 0;
			}

			if (m_MinPlayers > m_MaxPlayers)
			{
				m_MinPlayers = m_MaxPlayers;
			}

			if (m_LobbyPlayerPrefab != null)
			{
				var uv = m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>();
				if (uv == null)
				{
					m_LobbyPlayerPrefab = null;
					Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
				}
			}

			if (m_GamePlayerPrefab != null)
			{
				var uv = m_GamePlayerPrefab.GetComponent<NetworkIdentity>();
				if (uv == null)
				{
					m_GamePlayerPrefab = null;
					Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
				}
			}
		}

		NetworkLobbyPlayer GetLobbyPlayer (NetworkConnection conn) {
			NetworkLobbyPlayer oldLobbyPlayer = null;
			foreach (var lobbyPlayer in lobbySlots) {
				if (lobbyPlayer == null)
					continue;
				
				if (lobbyPlayer.serverId == conn.connectionId) {
					oldLobbyPlayer = lobbyPlayer;
				}
			}
			return oldLobbyPlayer;
		}

		Byte FindSlot()
		{
			for (byte i = 0; i < maxPlayers; i++)
			{
				if (lobbySlots[i] == null)
				{
					return i;
				}
			}
			return Byte.MaxValue;
		}

		void GameSceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
		{
			var lobbyPlayer = lobbyPlayerGameObject.GetComponent<NetworkLobbyPlayer>();
			if (lobbyPlayer == null)
			{
				// not a lobby player.. dont replace it
				return;
			}

			string loadedSceneName = SceneManager.GetSceneAt(0).name;
			if (LogFilter.logDebug) { Debug.Log("NetworkLobby SceneLoadedForPlayer scene:" + loadedSceneName + " " + conn); }

			if (lobbyScenes.Contains(loadedSceneName))
			{
				// cant be ready in lobby, add to ready list
				PendingPlayer pending;
				pending.conn = conn;
				pending.lobbyPlayer = lobbyPlayerGameObject;
				m_PendingPlayers.Add(pending);
				return;
			}

			var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
			var gamePlayer = OnLobbyServerCreateGamePlayer(conn, controllerId);
			if (gamePlayer == null)
			{
				// get start position from base class
				Transform startPos = GetStartPosition();
				if (startPos != null)
				{
					gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
				}
				else
				{
					gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
				}
			}

			if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
			{
				return;
			}

			// replace lobby player with game player
			NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
		}

		int CheckConnectionIsReadyToBegin(NetworkConnection conn)
		{
			int countPlayers = 0;
			for (int i = 0; i < conn.playerControllers.Count; i++)
			{
				var player = conn.playerControllers[i];
				if (player.IsValid)
				{
					var lobbyPlayer = GetLobbyPlayer (conn);
					if (lobbyPlayer.readyToBegin)
					{
						countPlayers += 1;
					}
				}
			}
			return countPlayers;
		}

		public void CheckReadyToBegin()
		{
			if (IsReadyToBegin ()) {
				m_PendingPlayers.Clear ();
				OnLobbyServerPlayersReady ();
			}
		}

		public bool IsReadyToBegin () {
			string loadedSceneName = SceneManager.GetSceneAt(0).name;
			if (!lobbyScenes.Contains(loadedSceneName))
			{
				return false;
			}

			int readyCount = ReadyPlayerCount();
			int playerCount = TotalPlayerCount();

			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				var conn = NetworkServer.connections[i];

				if (conn == null)
					continue;

				playerCount += 1;
				readyCount += CheckConnectionIsReadyToBegin(conn);
			}
			if (m_MinPlayers > 0 && readyCount < m_MinPlayers)
			{
				// not enough players ready yet.
				return false;
			}

			if (readyCount < playerCount)
			{
				// not all players are ready yet
				return false;
			}

			return true;
		}

		public int TotalPlayerCount () {
			int playerCount = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				var conn = NetworkServer.connections[i];

				if (conn == null)
					continue;

				playerCount++;
			}
			return playerCount;
		}

		public int ReadyPlayerCount () {
			int readyCount = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				var conn = NetworkServer.connections[i];

				if (conn == null)
					continue;

				readyCount += CheckConnectionIsReadyToBegin(conn);
			}
			return readyCount;
		}

		public void ServerReturnToLobby()
		{
			if (!NetworkServer.active)
			{
				Debug.Log("ServerReturnToLobby called on client");
				return;
			}
			ServerChangeScene(offlineScene);
		}

		void CallOnClientEnterLobby()
		{
			OnLobbyClientEnter();
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				var player = lobbySlots[i];
				if (player == null)
					continue;

				player.readyToBegin = false;
				player.OnClientEnterLobby();
			}
		}

		void CallOnClientExitLobby()
		{
			OnLobbyClientExit();
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				var player = lobbySlots[i];
				if (player == null)
					continue;

				player.OnClientExitLobby();
			}
		}

		public bool SendReturnToLobby()
		{
			if (client == null || !client.isConnected)
			{
				return false;
			}

			var msg = new EmptyMessage();
			client.Send(MsgType.LobbyReturnToLobby, msg);
			return true;
		}

		// ------------------------ server handlers ------------------------

		public override void OnServerConnect(NetworkConnection conn)
		{
			// numPlayers returns the player count including this one, so ok to be equal
			if (numPlayers > maxPlayers)
			{
				if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager can't accept new connection [" + conn + "], too many players connected."); }
				conn.Disconnect();
				return;
			}

//			// cannot join game in progress
//			string loadedSceneName = SceneManager.GetSceneAt(0).name;
//			if (!lobbyScenes.Contains(loadedSceneName))
//			{
//				if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager can't accept new connection [" + conn + "], not in lobby and game already in progress."); }
//				conn.Disconnect();
//				return;
//			}

			base.OnServerConnect(conn);
			OnLobbyServerConnect(conn);
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			// do this before base class does any destroys
			foreach (var player in conn.playerControllers) {
				OnServerRemovePlayer (conn, player);
			}

			base.OnServerDisconnect(conn);

			// if lobbyplayer for this connection has not been destroyed by now, then destroy it here
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				var player = lobbySlots[i];
				if (player == null)
					continue;

				if (player.connectionToClient == conn)
				{
					lobbySlots[i] = null;
					NetworkServer.Destroy(player.gameObject);
				}
			}

			OnLobbyServerDisconnect(conn);
		}

		public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
		{
			string loadedSceneName = SceneManager.GetSceneAt(0).name;
			if (!lobbyScenes.Contains(loadedSceneName))
			{
				return;
			}

			// check MaxPlayersPerConnection
			int numPlayersForConnection = 0;
			for (int i = 0; i < conn.playerControllers.Count; i++)
			{
				if (conn.playerControllers[i].IsValid)
					numPlayersForConnection += 1;
			}

			if (numPlayersForConnection >= maxPlayersPerConnection)
			{
				if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no more players for this connection."); }

				var errorMsg = new EmptyMessage();
				conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
				return;
			}

			byte slot = FindSlot();
			if (slot == Byte.MaxValue)
			{
				if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no space for more players"); }

				var errorMsg = new EmptyMessage();
				conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
				return;
			}

			var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
			if (newLobbyGameObject == null)
			{
				newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
			}

			var newLobbyPlayer = newLobbyGameObject.GetComponent<NetworkLobbyPlayer>();
			newLobbyPlayer.serverId = conn.connectionId;
			newLobbyPlayer.slot = slot;
			lobbySlots[slot] = newLobbyPlayer;
			newLobbyPlayer.colour = _colourPool.getColour ();
			newLobbyPlayer.nickname = "unknown";

			NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
		}

		public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
		{
			var playerControllerId = player.playerControllerId;

			NetworkLobbyPlayer oldLobbyPlayer = GetLobbyPlayer(conn);


			byte slot = oldLobbyPlayer.slot;
			_colourPool.releaseColour (oldLobbyPlayer.colour);
			Destroy (oldLobbyPlayer.gameObject);
			lobbySlots[slot] = null;
			base.OnServerRemovePlayer(conn, player);

			for (int i = 0; i < lobbySlots.Length; i++)
			{
				var lobbyPlayer = lobbySlots[i];
				if (lobbyPlayer != null)
				{
					lobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;

					s_LobbyReadyToBeginMessage.slotId = lobbyPlayer.slot;
					s_LobbyReadyToBeginMessage.readyState = false;
					NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, s_LobbyReadyToBeginMessage);
				}
			}

			OnLobbyServerPlayerRemoved(conn, playerControllerId);
		}

		public override void ServerChangeScene(string sceneName)
		{
			if (lobbyScenes.Contains(sceneName))
			{
				for (int i = 0; i < lobbySlots.Length; i++)
				{
					var lobbyPlayer = lobbySlots[i];
					if (lobbyPlayer == null)
						continue;

					// find the game-player object for this connection, and destroy it
					var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

					PlayerController playerController;
					if (NetworkCompat.Utils.GetPlayerController(uv.playerControllerId, uv.connectionToClient, out playerController))
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
			}
			base.ServerChangeScene(sceneName);
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			if (!lobbyScenes.Contains(sceneName))
			{
				// call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
				for (int i = 0; i < m_PendingPlayers.Count; i++)
				{
					var pending = m_PendingPlayers[i];
					GameSceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
				}
				m_PendingPlayers.Clear();
			}

			OnLobbyServerSceneChanged(sceneName);
		}

		void OnServerReadyToBeginMessage(NetworkMessage netMsg)
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReadyToBeginMessage"); }
			netMsg.ReadMessage(s_ReadyToBeginMessage);

			PlayerController lobbyController;
			if (! Utils.GetPlayerController((short)s_ReadyToBeginMessage.slotId, netMsg.conn, out lobbyController))
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + s_ReadyToBeginMessage.slotId); }
				return;
			}

			// set this player ready
			var lobbyPlayer = lobbyController.gameObject.GetComponent<NetworkLobbyPlayer>();
			lobbyPlayer.readyToBegin = s_ReadyToBeginMessage.readyState;

			// tell every player that this player is ready
			var outMsg = new LobbyReadyToBeginMessage();
			outMsg.slotId = lobbyPlayer.slot;
			outMsg.readyState = s_ReadyToBeginMessage.readyState;
			NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

			OnLobbyServerReadyToBegin (netMsg.conn);

			// maybe start the game
			CheckReadyToBegin();
		}

		void OnServerSceneLoadedMessage(NetworkMessage netMsg)
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnSceneLoadedMessage"); }

			netMsg.ReadMessage(s_SceneLoadedMessage);

			PlayerController lobbyController;
			if (!Utils.GetPlayerController((short)s_SceneLoadedMessage.value, netMsg.conn, out lobbyController))
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + s_SceneLoadedMessage.value); }
				return;
			}


			GameSceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
		}

		void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReturnToLobbyMessage"); }

			ServerReturnToLobby();
		}

		NetworkMessageDelegate _originalServerConnectHandler;
		public override void OnStartServer()
		{
			if (lobbyScenes.Count == 0)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NetworkLobbyMangaer"); }
				return;
			}

			if (string.IsNullOrEmpty(m_PlayScene))
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkLobbyMangaer"); }
				return;
			}

			if (lobbySlots.Length == 0)
			{
				lobbySlots = new NetworkLobbyPlayer[maxPlayers];
			}

			_colourPool = new ColourPool();

			Invoke ("SlyBastard", 1);

			NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
			NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
			NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);

			OnLobbyStartServer();
		}

		void SlyBastard () {
			Debug.Log ("Running Sly Bastard");
			foreach (var handler in NetworkServer.handlers) {
				if (handler.Key == MsgType.Connect) {
					_originalServerConnectHandler = handler.Value;
				}
			}

			NetworkServer.RegisterHandler (MsgType.Connect, OnServerConnectInternal);
		}

		void OnServerConnectInternal (NetworkMessage netMsg) {
			Debug.Log ("Sly Bastard Called");
			var tmp = networkSceneName;
			networkSceneName = lobbyScenes.First ();
			_originalServerConnectHandler (netMsg);
			networkSceneName = tmp;
		}

		public override void OnStartHost()
		{
			OnLobbyStartHost();
		}

		public override void OnStopHost()
		{
			OnLobbyStopHost();
		}

		// ------------------------ client handlers ------------------------


		public override void OnStartClient(NetworkClient lobbyClient)
		{
			if (lobbySlots.Length == 0)
			{
				lobbySlots = new NetworkLobbyPlayer[maxPlayers];
			}

			if (m_LobbyPlayerPrefab == null || m_LobbyPlayerPrefab.gameObject == null)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
			}
			else
			{
				ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
			}

			if (m_GamePlayerPrefab == null)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
			}
			else
			{
				ClientScene.RegisterPrefab(m_GamePlayerPrefab);
			}

			lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
			lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);

			OnLobbyStartClient(lobbyClient);
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			OnLobbyClientConnect(conn);
			CallOnClientEnterLobby();
			base.OnClientConnect(conn);
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			OnLobbyClientDisconnect(conn);
			base.OnClientDisconnect(conn);
		}

		public override void OnStopClient()
		{
			OnLobbyStopClient();
			CallOnClientExitLobby();
		}

		public override void OnClientSceneChanged(NetworkConnection conn)
		{
			string loadedSceneName = SceneManager.GetSceneAt(0).name;
			if (lobbyScenes.Contains(loadedSceneName))
			{
				if (client.isConnected)
				{
					CallOnClientEnterLobby();
				}
			}
			else
			{
				CallOnClientExitLobby();
			}

			base.OnClientSceneChanged(conn);
			OnLobbyClientSceneChanged(conn);
		}

		void OnClientReadyToBegin(NetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_LobbyReadyToBeginMessage);

			if (s_LobbyReadyToBeginMessage.slotId >= lobbySlots.Count())
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
				return;
			}

			var lobbyPlayer = lobbySlots[s_LobbyReadyToBeginMessage.slotId];
			if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
			{
				if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
				return;
			}

			lobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
			lobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
		}

		void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager Add Player failed."); }
			OnLobbyClientAddPlayerFailed();
		}

		// ------------------------ lobby server virtuals ------------------------

		public virtual void OnLobbyStartHost()
		{
		}

		public virtual void OnLobbyStopHost()
		{
		}

		public virtual void OnLobbyStartServer()
		{
		}

		public virtual void OnLobbyServerConnect(NetworkConnection conn)
		{
		}

		public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
		{
		}

		public virtual void OnLobbyServerSceneChanged(string sceneName)
		{
		}

		public virtual void OnLobbyServerReadyToBegin(NetworkConnection conn)
		{
		}

		public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
		{
			return null;
		}

		public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
		{
			return null;
		}

		public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
		{
		}

		// for users to apply settings from their lobby player object to their in-game player object
		public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
		{
			return true;
		}

		public virtual void OnLobbyServerPlayersReady()
		{
			// all players are readyToBegin, start the game
//			ServerChangeScene(m_PlayScene);
			// don't auto-start
		}

		// ------------------------ lobby client virtuals ------------------------

		public virtual void OnLobbyClientEnter()
		{
		}

		public virtual void OnLobbyClientExit()
		{
		}

		public virtual void OnLobbyClientConnect(NetworkConnection conn)
		{
		}

		public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
		{
		}

		public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
		{
		}

		public virtual void OnLobbyStopClient()
		{
		}

		public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
		{
		}

		// for users to handle adding a player failed on the server
		public virtual void OnLobbyClientAddPlayerFailed()
		{
		}

		// ------------------------ optional UI ------------------------

		void OnGUI()
		{
			if (!showLobbyGUI)
				return;

			string loadedSceneName = SceneManager.GetSceneAt(0).name;
			if (!lobbyScenes.Contains(loadedSceneName))
				return;

			Rect backgroundRec = new Rect(90 , 180, 500, 150);
			GUI.Box(backgroundRec, "Players:");

			if (NetworkClient.active)
			{
				Rect addRec = new Rect(100, 300, 120, 20);
				if (GUI.Button(addRec, "Add Player"))
				{
					TryToAddPlayer();
				}
			}
		}

		public void TryToAddPlayer()
		{
			if (NetworkClient.active)
			{
				short controllerId = -1;
				var controllers = NetworkClient.allClients[0].connection.playerControllers;

				if (controllers.Count < maxPlayers)
				{
					controllerId = (short)controllers.Count;
				}
				else
				{
					for (short i = 0; i < maxPlayers; i++)
					{
						if (!controllers[i].IsValid)
						{
							controllerId = i;
							break;
						}
					}
				}
				if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

				if (controllerId == -1)
				{
					if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager No Space!"); }
					return;
				}

				if (ClientScene.ready)
				{
					ClientScene.AddPlayer(controllerId);
				}
				else
				{
					ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
				}
			}
			else
			{
				if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager NetworkClient not active!"); }
			}
		}
	}
}