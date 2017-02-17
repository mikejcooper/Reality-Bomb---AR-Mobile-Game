using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;

public class PlayerCountTextController : ServerStateViewBehaviour {

	public Text playerCountText;


	protected override void OnServerStateChange (ProcessState state) {
		playerCountText.text = string.Format("{0}/{1} players ready", ServerSceneManager.Instance.ReadyPlayerCount, ServerSceneManager.Instance.ConnectedPlayerCount);
	}


}
