using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using GlobalNetworking;
namespace ClientNetworking
{
	
	public class DiscoveryClient : NetworkDiscovery {
		
		public delegate void ServerDiscovered(string address);

		public event ServerDiscovered serverDiscoveryEvent;

		public void ListenForServers () {
			showGUI = false;
			broadcastKey = NetworkConstants.BROADCAST_KEY;
			isClient = true;
			useNetworkManager = false;
			Initialize();
		}


		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			if (serverDiscoveryEvent != null) {
				serverDiscoveryEvent (fromAddress);
			}
		}
	}

}

