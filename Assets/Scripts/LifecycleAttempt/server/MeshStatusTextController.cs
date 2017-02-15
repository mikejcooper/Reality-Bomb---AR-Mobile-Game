using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ServerLifecycle;

public class MeshStatusTextController : ServerStateViewBehaviour {

	public Text meshStatusText;


	protected override void OnServerStateChange (ProcessState state) {
		string meshStatus = "";

//		if (serverSceneManager.currentMesh != null) {
//			meshStatus += "we have a mesh";
//		} else {
//			meshStatus += "we don't have a mesh";
//		}

		switch (ServerSceneManager.Instance.meshRetrievalState) {
		case ServerSceneManager.MeshRetrievalState.Idle:
			meshStatus += " and we're not getting a new one";
			break;
		case ServerSceneManager.MeshRetrievalState.Retrieving:
			meshStatus += " and we're getting a new one";
			break;
		}

		meshStatusText.text = meshStatus;
	}


}
