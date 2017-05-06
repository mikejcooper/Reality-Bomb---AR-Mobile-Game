using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalNetworking
{
	public class Flags
	{
		// game server is never localhost on a mobile device
		public static bool GAME_SERVER_IS_LOCALHOST = !Application.isMobilePlatform && true;
	}
}