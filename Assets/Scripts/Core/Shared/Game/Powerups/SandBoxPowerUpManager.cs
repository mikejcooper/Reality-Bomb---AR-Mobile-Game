using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {
	
	public class SandBoxPowerUpManager : BasePowerUpManager {

		public GameObject PlaneObject;

		protected override void Start () {
			base.Start ();
			OnMeshReady (PlaneObject);	
		}

		protected override bool IsAllowedToSpawn(){
			return true;
		}
	}

}