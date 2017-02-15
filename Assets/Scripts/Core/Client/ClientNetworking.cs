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

		void Start()
		{
			showGUI = false;
			broadcastKey = NetworkConstants.BROADCAST_KEY;

		}

		public void ListenForServers () {
			// Initialize needs to be called before StartAsClient every time
			Initialize();
			StartAsClient ();
		}


		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			serverDiscoveryEvent (fromAddress);
		}
	}

}

