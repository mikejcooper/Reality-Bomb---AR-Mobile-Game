using System;
using UnityEngine;

namespace GlobalNetworking
{
	public class NetworkConstants
	{
		public static int BROADCAST_KEY = 3110;
		public static int MESH_BROADCAST_PORT = 3110;
		public static int MESH_TRANSFER_PORT = 3111;
		public static int GAME_PORT = 4812;
		public static short MSG_GET_MESH = 928;
		public static short MSG_START_GAME_COUNTDOWN = 981;
		public static short MSG_CANCEL_GAME_COUNTDOWN = 982;

	}
}

