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

		public delegate void OnCarDetailsUpdate();
		public event OnCarDetailsUpdate OnCarDetailsUpdateEvent = delegate {};

		public struct GameResult
		{
			public float FinishTime;
			public int FinishPosition;
			public int TotalPlayers;
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
		public string m_Nickname;
		[SyncVar]
		public int m_Colour;
		[SyncVar]
		public int m_VehicleId;
		[SyncVar]
		public SyncListGameResult m_GameResults = new SyncListGameResult();

		public int serverId { get { return m_ServerId; } set { m_ServerId = value; }}
		public byte slot { get { return m_Slot; } set { m_Slot = value; }}
		public bool readyToBegin { get { return m_ReadyToBegin; } set { m_ReadyToBegin = value; if (!m_ReadyToBegin) { m_PlayingGame = m_GameLoaded = false; } } }
		public bool playingGame { get { return m_PlayingGame; } set { m_PlayingGame = value; } }
		public bool gameLoaded { get { return m_GameLoaded; } set { m_GameLoaded = value; } }
		public string nickname { get { return m_Nickname; } set { m_Nickname = value; } }
		public int colour { get { return m_Colour; } set { m_Colour = value; } }
		public int vehicleId { get { return m_VehicleId; } set { m_VehicleId = value; } }
		public SyncListGameResult gameResults { get { return m_GameResults; } }
		public GameResult lastGameResult { get { return m_GameResults [m_GameResults.Count - 1]; } }

		public int totalCumulativeGamesScore () {
			int totalCumulativeScore = 0;
			foreach (var gameResult in m_GameResults) {
				var gameScore = gameResult.TotalPlayers - gameResult.FinishPosition - 1;
				totalCumulativeScore += gameScore;
			}
			return totalCumulativeScore;
		}

		private void OnCarColourUpdate ( int ignore) {
			OnCarDetailsUpdateEvent ();
		}

		private void OnCarVehicleIdUpdate ( int ignore) {
			OnCarDetailsUpdateEvent ();
		}

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
			CmdSetDetails (ClientSceneManager.Instance.ClientNickName, ClientSceneManager.Instance.ClientVehicleId);
		}

		[Command]
		public void CmdSetDetails (string nickname, int vehicleId) {
			Debug.Log (string.Format ("received request to set name to {0} and vehicle id to {1}", nickname, vehicleId));
			m_Nickname = nickname;
			m_VehicleId = vehicleId;
			SetDirtyBit (2);
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