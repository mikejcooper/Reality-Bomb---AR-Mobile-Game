﻿using UnityEngine.Networking;
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
		[SerializeField] public bool ShowLobbyGUI = true;

		byte m_Slot;
		bool m_ReadyToBegin;
		bool m_GameLoaded;

		public byte slot { get { return m_Slot; } set { m_Slot = value; }}
		public bool readyToBegin { get { return m_ReadyToBegin; } set { m_ReadyToBegin = value; } }
		public bool gameLoaded { get { return m_GameLoaded; } set { m_GameLoaded = value; } }

		void Start()
		{
			DontDestroyOnLoad(gameObject);
		}

		public override void OnStartClient()
		{
			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				lobby.lobbySlots[m_Slot] = this;
				m_ReadyToBegin = false;
				gameLoaded = false;
				OnClientEnterLobby();
			}
			else
			{
				Debug.LogError("LobbyPlayer could not find a NetworkLobbyManager. The LobbyPlayer requires a NetworkLobbyManager object to function. Make sure that there is one in the scene.");
			}
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

		void OnLevelWasLoaded()
		{
			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				// dont even try this in the startup scene
				string loadedSceneName = SceneManager.GetSceneAt(0).name;
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

		// ------------------------ Custom Serialization ------------------------

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			// dirty flag
			writer.WritePackedUInt32(1);

			writer.Write(m_Slot);
			writer.Write(m_ReadyToBegin);
			writer.Write (m_GameLoaded);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			var dirty = reader.ReadPackedUInt32();
			if (dirty == 0)
				return;

			m_Slot = reader.ReadByte();
			m_ReadyToBegin = reader.ReadBoolean();
			m_GameLoaded = reader.ReadBoolean ();
		}

		// ------------------------ optional UI ------------------------

		void OnGUI()
		{
			if (!ShowLobbyGUI)
				return;

			var lobby = NetworkManager.singleton as NetworkLobbyManager;
			if (lobby)
			{
				if (!lobby.showLobbyGUI)
					return;

				string loadedSceneName = SceneManager.GetSceneAt(0).name;
				if (!lobby.lobbyScenes.Contains(loadedSceneName))
					return;
			}

			Rect rec = new Rect(100 + m_Slot * 100, 200, 90, 20);

			if (isLocalPlayer)
			{
				string youStr;
				if (m_ReadyToBegin)
				{
					youStr = "(Ready)";
				}
				else
				{
					youStr = "(Not Ready)";
				}
				GUI.Label(rec, youStr);

				if (m_ReadyToBegin)
				{
					rec.y += 25;
					if (GUI.Button(rec, "STOP"))
					{
						SendNotReadyToBeginMessage();
					}
				}
				else
				{
					rec.y += 25;
					if (GUI.Button(rec, "START"))
					{
						SendReadyToBeginMessage();
					}

					rec.y += 25;
					if (GUI.Button(rec, "Remove"))
					{
						ClientScene.RemovePlayer(GetComponent<NetworkIdentity>().playerControllerId);
					}
				}
			}
			else
			{
				GUI.Label(rec, "Player [" + netId + "]");
				rec.y += 25;
				GUI.Label(rec, "Ready [" + m_ReadyToBegin + "]");
			}
		}
	}
}