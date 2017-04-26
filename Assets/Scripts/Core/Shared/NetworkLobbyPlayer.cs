using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace NetworkCompat {
	
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkLobbyPlayer")]
	public class NetworkLobbyPlayer : NetworkBehaviour
	{

		public struct GameResult
		{
			public float FinishTime;
			public int FinishPosition;
		}

		public class SyncListGameResult : SyncListStruct<GameResult>
		{
		}

		[SyncVar]
		public int m_ServerId;
		[SyncVar]
		public byte m_Slot;
		[SyncVar]
		public bool m_ReadyToBegin;
		[SyncVar]
		public bool m_PlayingGame;
		[SyncVar]
		public bool m_GameLoaded;
		[SyncVar]
		public string m_Name;
		[SyncVar]
		public int m_Colour;
		[SyncVar]
		public SyncListGameResult m_GameResults = new SyncListGameResult();

		public int serverId { get { return m_ServerId; } set { m_ServerId = value; }}
		public byte slot { get { return m_Slot; } set { m_Slot = value; }}
		public bool readyToBegin { get { return m_ReadyToBegin; } set { m_ReadyToBegin = value; if (!m_ReadyToBegin) { m_PlayingGame = m_GameLoaded = false; } } }
		public bool playingGame { get { return m_PlayingGame; } set { m_PlayingGame = value; } }
		public bool gameLoaded { get { return m_GameLoaded; } set { m_GameLoaded = value; } }
		public string name { get { return m_Name; } set { m_Name = value; } }
		public int colour { get { return m_Colour; } set { m_Colour = value; } }
		public SyncListGameResult gameResults { get { return m_GameResults; } }
		public GameResult lastGameResult { get { return m_GameResults [m_GameResults.Count - 1]; } }

		public void AddGameResult (GameResult result) {
			m_GameResults.Add (result);
		}

		void Start()
		{
			DontDestroyOnLoad(gameObject);
		}

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		void OnActiveChange (string newVal) {
			Debug.LogError("var change to " + newVal );
		}

		public override void OnStartClient()
		{
			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				lobby.lobbySlots[m_Slot] = this;
				m_ReadyToBegin = false;
				OnClientEnterLobby();
			}
			else
			{
				Debug.LogError("LobbyPlayer could not find a NetworkLobbyManager. The LobbyPlayer requires a NetworkLobbyManager object to function. Make sure that there is one in the scene.");
			}
		}

		public override void OnStartLocalPlayer() {
			CmdSetName (ClientSceneManager.Instance.ClientNickName);
		}

		[Command]
		public void CmdSetName (string name) {
			m_Name = name;
		}

		public void SendReadyToBeginMessage()
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendReadyToBeginMessage"); }

			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				var msg = new LobbyReadyToBeginMessage();
				msg.slotId = (byte)playerControllerId;
				msg.readyState = true;
				lobby.client.Send(MsgType.LobbyReadyToBegin, msg);
			}
		}

		public void SendNotReadyToBeginMessage()
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendReadyToBeginMessage"); }

			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				var msg = new LobbyReadyToBeginMessage();
				msg.slotId = (byte)playerControllerId;
				msg.readyState = false;
				lobby.client.Send(MsgType.LobbyReadyToBegin, msg);
			}
		}

		public void SendSceneLoadedMessage()
		{
			if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer SendSceneLoadedMessage"); }

			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				var msg = new IntegerMessage(playerControllerId);
				lobby.client.Send(MsgType.LobbySceneLoaded, msg);
			}
		}

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				// dont even try this in the startup scene
				// Should we check if the LoadSceneMode is Single or Additive??
				// Can the lobby scene be loaded Additively??
				string loadedSceneName = scene.name;
				if (lobby.lobbyScenes.Contains(loadedSceneName))
				{
					return;
				}
			}

			if (isLocalPlayer)
			{
				SendSceneLoadedMessage();
			}
		}

		public void RemovePlayer()
		{
			if (isLocalPlayer && !m_ReadyToBegin)
			{
				if (LogFilter.logDebug) { Debug.Log("NetworkLobbyPlayer RemovePlayer"); }

				ClientScene.RemovePlayer(GetComponent<NetworkIdentity>().playerControllerId);
			}
		}

		// ------------------------ callbacks ------------------------

		public virtual void OnClientEnterLobby()
		{
		}

		public virtual void OnClientExitLobby()
		{
		}

		public virtual void OnClientReady(bool readyState)
		{
		}


	}
}