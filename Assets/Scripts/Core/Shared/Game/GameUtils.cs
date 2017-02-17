using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
	public static int ChooseRandomPlayerConnectionId () {
		List<int> activeConnectionIds = new List<int>();
		for (int i=0; i<UnityEngine.Networking.NetworkServer.connections.Count; i++) {
			if (UnityEngine.Networking.NetworkServer.connections [i] != null &&
				UnityEngine.Networking.NetworkServer.connections [i].isConnected) {
				activeConnectionIds.Add (UnityEngine.Networking.NetworkServer.connections [i].connectionId);
			}
		}
		int bombPlayerIndex = UnityEngine.Random.Range(0, activeConnectionIds.Count);
		DebugConsole.Log (string.Format ("chose index {0} from connections size of {1}", bombPlayerIndex, activeConnectionIds.Count));

		return activeConnectionIds [bombPlayerIndex];
	}


}


