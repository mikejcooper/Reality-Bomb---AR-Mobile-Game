using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class ShieldAbilityProperties : BaseAbilityProperties {
		public GameObject ShieldPrefab;
	}

    public class ShieldAbilitySetup : BaseAbilitySetup
    {
        public ShieldAbilitySetup(Canvas canvas, ShieldAbilityProperties properties) : base(properties)
        {

        }
    }

	public class ShieldAbility : BaseAbility<ShieldAbilitySetup> {

		public const string TAG = "shield";

		private GameObject _shieldObject;

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {

				// Instantiate and keep track of the new instance
				_shieldObject = Instantiate(((ShieldAbilityProperties)_abilitySetup.AbilityProperties).ShieldPrefab);

				// Set parent
				_shieldObject.transform.SetParent(transform, false);
				_shieldObject.transform.localPosition = Vector3.zero;

			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				Destroy (_shieldObject);
			}
		}

		public override string GetTag () {
			return TAG;
		}
	}
}