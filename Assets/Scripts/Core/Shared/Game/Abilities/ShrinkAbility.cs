using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class ShrinkAbilityProperties : BaseAbilityProperties {}

    public class ShrinkAbilitySetup : BaseAbilitySetup
    {
        public ShrinkAbilitySetup(Canvas canvas, ShrinkAbilityProperties properties) : base(properties)
        {

        }
    }

    public class ShrinkAbility : BaseAbility<ShrinkAbilitySetup> {

		public const string TAG = "shrinkability";
		private const float TURN_FACTOR = 1.5f;
		private const float SPEED_FACTOR = 0.75f;
		private const float SCALE_FACTOR = 0.5f;

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