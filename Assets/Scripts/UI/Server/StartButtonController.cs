using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;

public class StartButtonController : ServerStateViewBehaviour {


	public Selectable button;
	public Text text;

	protected override void OnServerStateChange (ProcessState state) {
		Debug.Log (state);
		switch (state) {
		case ProcessState.PreparingGame:
			text.text = "Start Game";
			button.enabled = true;
			break;
		case ProcessState.CountingDown:
			text.text = "Cancel Game";
			button.enabled = true;
			break;
		default:
			button.enabled = false;
			break;
		}
	}
	

}
