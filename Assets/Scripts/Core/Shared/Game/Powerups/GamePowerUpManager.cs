using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {

	public class GamePowerUpManager: BasePowerUpManager {

		public GameObject PlaneObject;
		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties InkProperties;
		public GameManager GameManager;

		protected override void Start () {
			base.Start ();
			OnMeshReady (PlaneObject);	
		}

		override protected PowerupDefinition[] GetAvailablePowerups () {
			return new PowerupDefinition[] { 
				new PowerupDefinition (typeof(SpeedAbility), SpeedProperties),
				new PowerupDefinition (typeof(SandboxInkAbility), InkProperties)
			};
		}

		public override void OnPowerUpStart<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'Main Game PUM': Speed boost activated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				Debug.Log ("'Main Game PUM':Ink splatter activated");
			}
		}

		public override void OnPowerUpStop<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'Main Game PUM':Speed boost deactivated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				Debug.Log ("'Main Game PUM':Ink splatter deactivated");
			}
		}
	}

}