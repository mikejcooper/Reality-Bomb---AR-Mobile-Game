using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class GrowAbilityProperties : BaseAbilityProperties {}

	public class GrowAbility : BaseAbility<GrowAbilityProperties> {

		public const string TAG = "growability";

		private const float TURN_FACTOR = 0.5f;
		private const float SPEED_FACTOR = 1.5f;
		private const float SCALE_FACTOR = 2f;

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				properties.Scale *= SCALE_FACTOR;
				properties.SpeedLimit *= SPEED_FACTOR;
				properties.Accel *= SPEED_FACTOR;
				properties.TurnRate *= TURN_FACTOR;
			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				properties.Scale /= SCALE_FACTOR;
				properties.SpeedLimit /= SPEED_FACTOR;
				properties.Accel /= SPEED_FACTOR;
				properties.TurnRate /= TURN_FACTOR;
			}
		}
			

		public override string GetTag () {
			return TAG;
		}

	}
}