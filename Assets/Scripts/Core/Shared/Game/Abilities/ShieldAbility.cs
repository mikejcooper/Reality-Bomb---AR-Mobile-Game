using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class ShieldAbilityProperties : BaseAbilityProperties {
		public GameObject ShieldPrefab;
	}


	public class ShieldAbility : BaseAbility<ShieldAbilityProperties> {

		public const string TAG = "shield";

		private Shield _component;

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				_component = properties.gameObject.AddComponent<Shield> ();
				_component.ShieldPrefab = _abilityProperties.ShieldPrefab;
			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				Destroy (_component);
			}
		}

		public override string GetTag () {
			return TAG;
		}
	}
}