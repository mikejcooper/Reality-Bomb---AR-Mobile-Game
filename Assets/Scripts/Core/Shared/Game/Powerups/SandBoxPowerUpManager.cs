using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {
	
	public class SandBoxPowerUpManager : BasePowerUpManager {

		public GameObject PlaneObject;
		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties InkProperties;
		public ShieldAbilityProperties ShieldProperties;

		// Events
		public delegate void OnSpeedBoostActivated ();
		public delegate void OnInkSplatterActivated ();
		public delegate void OnShieldActivated ();
		public event OnSpeedBoostActivated SpeedBoostActivatedEvent;		
		public event OnInkSplatterActivated InkSplatterActivatedEvent;
		public event OnShieldActivated ShieldActivatedEvent;

		protected override void Start () {
			base.Start ();
			OnMeshReady (PlaneObject);	
		}

		override protected PowerupDefinition[] GetAvailablePowerups () {
			return new PowerupDefinition[] { 
				new PowerupDefinition (typeof(SpeedAbility), SpeedProperties),
				new PowerupDefinition (typeof(SandboxInkAbility), InkProperties),
				new PowerupDefinition (typeof(ShieldAbility), ShieldProperties)
			};
		}

		public override void OnPowerUpStart<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'SBPUM': Speed boost activated");
				SpeedBoostActivatedEvent ();
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				InkSplatterActivatedEvent ();
				Debug.Log ("'SBPUM':Ink splatter activated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(ShieldAbility))) {
				ShieldActivatedEvent ();
				Debug.Log ("'SBPUM': Shield activated");
			}
		}

		public override void OnPowerUpStop<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'SBPUM':Speed boost deactivated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				Debug.Log ("'SBPUM':Ink splatter deactivated");
			}  else if (ability.GetType ().IsAssignableFrom (typeof(ShieldAbility))) {
				Debug.Log ("'SBPUM': Shield deactivated");
			}
		}

		protected override bool IsAllowedToSpawn(){
			return true;
		}
	}

}