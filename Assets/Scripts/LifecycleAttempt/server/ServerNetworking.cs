using System;
using UnityEngine.Networking;
using UnityEngine;
using UnityThreading;
using System.Collections;
using WebSocketSharp;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking.NetworkSystem;



namespace ServerNetworking
{


	public class DiscoveryServer : NetworkDiscovery {

		private const int BROADCAST_KEY = 3110;
	
		void Start()
		{
			showGUI = false;
			Application.runInBackground = true;
			broadcastKey = BROADCAST_KEY;
			Initialize();
			StartAsServer();
		}
			
	}

	public class CommunicationServer {


		public delegate void ClientConnectedCallback();
		public delegate void ClientDisconnectedCallback();

		public event ClientConnectedCallback clientConnectedCallback;
		public event ClientDisconnectedCallback clientDisconnectedCallback;

		private const int BROADCAST_KEY = 3110;
		public const int CONNECTION_PORT = 7777;

		public CommunicationServer () {

			ConnectionConfig netConfig = new ConnectionConfig();
			netConfig.AddChannel(QosType.Reliable);

			HostTopology topology = new HostTopology  (netConfig, 10000);
			NetworkServer.Configure (topology);

			NetworkServer.Listen(CONNECTION_PORT);	
			NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnect);
			NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);

			NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnSceneLoaded);
			NetworkServer.RegisterHandler(MsgType.Ready, OnSceneLoaded);

		}
			

		public void OnSceneLoaded (NetworkMessage netMsg) {
			Debug.Log ("OnSceneLoaded");

			// force the client to be ready
			NetworkServer.SetClientReady (netMsg.conn);

			// spawn all network identity objects that are in our scene already
			NetworkServer.SpawnObjects ();
		}

		public void ChangeClientsScene (string sceneName) {
			NetworkServer.SendToAll (MsgType.Scene, new StringMessage (sceneName));
		}

		public void OnClientConnect(NetworkMessage netMsg) {
			clientConnectedCallback ();
		}

		public void OnClientDisconnect (NetworkMessage netMsg) {
			clientDisconnectedCallback ();
		}


	}

	public class MeshDiscoveryServer {

		public delegate void MeshServerDiscoveredCallback(string address, int port);

		public event MeshServerDiscoveredCallback meshServerDiscoveredCallback;

		private bool searching = false;
		private UnityThreading.ActionThread searchThread;
		public MeshDiscoveryServer () {
			
		}

		public void StartSearching () {
			searching = true;
			searchThread = UnityThreadHelper.CreateThread(ListenForBroadcasts);
		}

		public void StopSearching () {
			searching = false;
			if (searchThread != null) {
				searchThread.Exit ();
			}
		}

		private void ListenForBroadcasts()
		{
			int port = 3110;

			try {
				var client = new UdpClient (port);

				while (searching) {
					try {
						IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
						byte[] data = client.Receive (ref anyIP);

						string text = Encoding.UTF8.GetString (data);

						if (text == "RealityBomb") {
							// we've found our server
							meshServerDiscoveredCallback(anyIP.Address.ToString (), port + 1);
						}

					} catch (Exception err) {
						Debug.LogError (err.ToString ());
					}

				}


			} catch (SocketException e) {
				Debug.Log ("broadcast socket is already in use, assuming broadcast is coming from localhost");
				meshServerDiscoveredCallback("localhost", port + 1);
			}

		}
	}
}

