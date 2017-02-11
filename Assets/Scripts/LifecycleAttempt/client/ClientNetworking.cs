using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

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
		public delegate void ServerDisconnectedCallback();
		public delegate void ChangeSceneCallback(string sceneName);

		public event ServerConnectedCallback serverConnectedEvent;
		public event ServerDisconnectedCallback serverDisconnectedEvent;
		public event ChangeSceneCallback changeSceneEvent;

		public void ConnectToAddress (string address) {

			RegisterHandler(MsgType.Connect, OnClientConnect);
			RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
			RegisterHandler(MsgType.Error, OnClientDisconnect);
			RegisterHandler (MsgType.Scene, OnServerRequestSceneChange);

			ConnectionConfig netConfig = new ConnectionConfig();
			netConfig.AddChannel(QosType.Reliable);

			HostTopology topology = new HostTopology  (netConfig, 10000);
			Configure (topology);

			Connect(address, CONNECTION_PORT);


		}

		public void NotifySceneLoaded (string sceneName, GameObject prefab) {
			Debug.Log ("NotifySceneLoaded");

			// tell server we're ready
			ClientScene.Ready (connection);
			Send (MsgType.LobbySceneLoaded, new StringMessage (sceneName));
		}

		public void OnServerRequestSceneChange(NetworkMessage netMsg) {
			
			string sceneName = netMsg.ReadMessage<StringMessage>().value;
			changeSceneEvent (sceneName);
		}

		public void OnClientConnect(NetworkMessage netMsg) {
			serverConnectedEvent ();
		}

		public void OnClientDisconnect(NetworkMessage netMsg) {
			serverDisconnectedEvent ();
		}

	}
}

