using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {
	
	[System.Serializable]
	public class SpeedAbilityProperties : BaseAbilityProperties {}

	public class SpeedAbility : BaseAbility<SpeedAbilityProperties> {

		public const string TAG = "speed";

		override protected void OnApplyAbilitySelf (CarProperties properties, Canvas canvas) {
			properties.MaxSpeed *= 2.0f;
			properties.Acceleration *= 2.0f;
		}

		override protected void OnRemoveAbilitySelf (CarProperties properties, Canvas canvas) {
			properties.MaxSpeed /= 2.0f;
			properties.Acceleration /= 2.0f;
		}

		public override string GetTag () {
			return TAG;
		}
			
	}

}