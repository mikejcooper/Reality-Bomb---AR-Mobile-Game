using System;
using UnityEngine.Networking;
using UnityEngine;

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
			NetworkServer.Listen(CONNECTION_PORT);	
			NetworkServer.RegisterHandler(MsgType.Ready, OnClientConnect);
			NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
		}

		public void OnClientConnect(NetworkMessage netMsg) {
			clientConnectedCallback ();
		}

		public void OnClientDisconnect (NetworkMessage netMsg) {
			clientDisconnectedCallback ();
		}


	}
}

