using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {
	
	public class SandBoxPowerUpManager : BasePowerUpManager {

		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties SandboxInkProperties;
		public ShieldAbilityProperties ShieldProperties;
		public GrowAbilityProperties GrowProperties;
		public ShrinkAbilityProperties ShrinkProperties;

		public delegate void OnSpeedBoostActivated();
		public delegate void OnInkSplatterActivated();
		public delegate void OnShieldActivated();
		public delegate void OnGrowActivated();
		public delegate void OnShrinkActivated();
		public event OnSpeedBoostActivated SpeedBoostActivatedEvent = delegate {};
		public event OnInkSplatterActivated InkSplatterActivatedEvent = delegate {};
		public event OnShieldActivated ShieldActivatedEvent = delegate {};
		public event OnGrowActivated GrowActivatedEvent = delegate {};
		public event OnShrinkActivated ShrinkActivatedEvent = delegate{};

		public GameObject PlaneObject;

		protected override void Start () {
			base.Start ();
			OnMeshReady (PlaneObject);	
		}

		protected override PowerupDefinition[] GetAvailablePowerups () {
			return new PowerupDefinition[] {
				new PowerupDefinition (typeof(SpeedAbility), SpeedAbility.TAG, SpeedProperties),
				new PowerupDefinition (typeof(SandboxInkAbility), SandboxInkAbility.TAG, SandboxInkProperties),
				new PowerupDefinition (typeof(ShieldAbility), ShieldAbility.TAG, ShieldProperties),
				new PowerupDefinition (typeof(GrowAbility), GrowAbility.TAG, GrowProperties),
				new PowerupDefinition (typeof(ShrinkAbility), ShrinkAbility.TAG, ShrinkProperties)
			};
		}

		public override void OnAbilityStart (string abilityTag) {
			base.OnAbilityStart(abilityTag);
			switch (abilityTag) {
			case SandboxInkAbility.TAG:
				InkSplatterActivatedEvent ();
				break;
			case SpeedAbility.TAG:
				SpeedBoostActivatedEvent ();
				break;
			case ShieldAbility.TAG:
				ShieldActivatedEvent ();
				break;
			case GrowAbility.TAG:
				GrowActivatedEvent ();
				break;
			case ShrinkAbility.TAG:
				ShrinkActivatedEvent ();
				break;
			}
		}
			
		protected override bool IsAllowedToSpawn(){
			return true;
		}
	}

}