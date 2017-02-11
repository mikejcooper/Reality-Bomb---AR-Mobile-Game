using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;

public class StartButtonController : ServerStateViewBehaviour {


	public Selectable button;

	protected override void OnServerStateChange (ProcessState state) {
		Debug.Log (state);
		switch (state) {
		case ProcessState.PreparingGame:
			button.enabled = true;
			break;
		default:
			button.enabled = false;
			break;
		}
	}
	

}
