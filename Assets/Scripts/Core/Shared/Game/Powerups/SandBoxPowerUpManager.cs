using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {
	
	public class SandBoxPowerUpManager : BasePowerUpManager {

		public GameObject PlaneObject;
		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties InkProperties;
		public SandboxManager SB_Manager;

		public delegate void OnSpeedBoostActivated ();
		public static event OnSpeedBoostActivated SpeedBoostActivated;

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
				Debug.Log ("'SBPUM': Speed boost activated");
				SB_Manager.SetSpeedTxt ();
				SpeedBoostActivated ();
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				SB_Manager.SetSplatTxt ();
				Debug.Log ("'SBPUM':Ink splatter activated");
			}
		}

		public override void OnPowerUpStop<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'SBPUM':Speed boost deactivated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				Debug.Log ("'SBPUM':Ink splatter deactivated");
			}
		}
	}

}