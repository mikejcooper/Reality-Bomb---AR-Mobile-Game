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
using GlobalNetworking;


namespace ServerNetworking
{


	public class DiscoveryServer : NetworkDiscovery {
	
		void Start()
		{
			showGUI = false;
			Application.runInBackground = true;
			broadcastKey = NetworkConstants.BROADCAST_KEY;
			Initialize();
			StartAsServer();
		}
			
	}

	public class MeshDiscoveryServer {

		public delegate void MeshServerDiscovered(string address, int port);

		public event MeshServerDiscovered MeshServerDiscoveredEvent;

		private bool _searching = false;
		private UnityThreading.ActionThread _searchThread;

		public void StartSearching () {
			_searching = true;
			_searchThread = UnityThreadHelper.CreateThread(ListenForBroadcasts);
		}

		public void StopSearching () {
			_searching = false;
			if (_searchThread != null) {
				_searchThread.Exit ();
			}
		}

		private void ListenForBroadcasts()
		{

			try {
				var client = new UdpClient (NetworkConstants.MESH_BROADCAST_PORT);

				while (_searching) {
					try {
						IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
						byte[] data = client.Receive (ref anyIP);

						string text = Encoding.UTF8.GetString (data);

						if (text == "RealityBomb") {
							// we've found our server
							UnityThreadHelper.Dispatcher.Dispatch (() => {
								if (MeshServerDiscoveredEvent != null)
									MeshServerDiscoveredEvent(anyIP.Address.ToString (), NetworkConstants.MESH_TRANSFER_PORT);
							});
						}

					} catch (Exception err) {
						Debug.LogError (err.ToString ());
					}

				}


			} catch (SocketException) {
				Debug.Log ("broadcast socket is already in use, assuming broadcast is coming from localhost");
				UnityThreadHelper.Dispatcher.Dispatch (() => {
					if (MeshServerDiscoveredEvent != null)
						MeshServerDiscoveredEvent ("localhost", NetworkConstants.MESH_TRANSFER_PORT);
				});
			}

		}
	}
}

