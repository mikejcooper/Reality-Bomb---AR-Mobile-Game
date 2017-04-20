using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class ShrinkAbilityProperties : BaseAbilityProperties {}

	public class ShrinkAbility : BaseAbility<ShrinkAbilityProperties> {

		public const string TAG = "shrinkability";
		private const float SPEED_FACTOR = 0.75f;
		private const float SCALE_FACTOR = 0.5f;

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				properties.Scale *= SCALE_FACTOR;

				properties.SpeedLimit *= SPEED_FACTOR;
				properties.Accel *= SPEED_FACTOR;

			}

		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				properties.Scale /= SCALE_FACTOR;

				properties.SpeedLimit /= SPEED_FACTOR;
				properties.Accel /= SPEED_FACTOR;
			}
		}


		public override string GetTag () {
			return TAG;
		}

	}
}