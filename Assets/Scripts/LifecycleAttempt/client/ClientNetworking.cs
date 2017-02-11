using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ClientNetworking
{
	
	public class DiscoveryClient : NetworkDiscovery {

		private const int BROADCAST_KEY = 3110;
	

		public delegate void ServerDiscoveredCallback(string address);

		public event ServerDiscoveredCallback serverDiscoveryEvent;

		void Start()
		{
			showGUI = false;
			broadcastKey = BROADCAST_KEY;
			Initialize();
		}

		public void ListenForServers () {
			StartAsClient ();
		}


		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			serverDiscoveryEvent (fromAddress);
		}
	}

	public class CommunicationClient : NetworkClient {

		private const int BROADCAST_KEY = 3110;
		public const int CONNECTION_PORT = 7777;

		public delegate void ServerConnectedCallback();

		public event ServerConnectedCallback serverConnectedEvent;

		public delegate void ServerDisconnectedCallback();

		public event ServerDisconnectedCallback serverDisconnectedEvent;

		public void ConnectToAddress (string address) {

			RegisterHandler(MsgType.Connect, OnClientConnect);
			RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
			RegisterHandler(MsgType.Error, OnClientDisconnect);

			Connect(address, CONNECTION_PORT);

		}

		public void OnClientConnect(NetworkMessage netMsg) {
			serverConnectedEvent ();
		}

		public void OnClientDisconnect(NetworkMessage netMsg) {
			serverDisconnectedEvent ();
		}

	}
}

