using System;
using UnityEngine;

namespace GlobalNetworking
{
	public class NetworkConstants
	{
		public static int BROADCAST_KEY = 3110;
		public static int MESH_BROADCAST_PORT = 3110;
		public static int MESH_TRANSFER_PORT = 3111;
		public static int GAME_PORT = 7777;
		public static short MSG_GET_MESH = 928;
		public static short MSG_GAME_LOADED = 182;
		public static short MSG_PLAYER_DATA_UPDATE = 891;
		public static short MSG_PLAYER_DATA_ID = 892;
	}
}

