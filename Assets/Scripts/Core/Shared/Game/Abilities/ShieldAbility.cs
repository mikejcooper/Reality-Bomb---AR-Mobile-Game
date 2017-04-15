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

		override protected void OnApplyAbilitySelf (CarProperties properties, Canvas canvas) {

			_component = properties.gameObject.AddComponent<Shield> ();
			_component.ShieldPrefab = _abilityProperties.ShieldPrefab;

		}

		override protected void OnRemoveAbilitySelf (CarProperties properties, Canvas canvas) {
			_component.Destroy ();
			Destroy (_component);
		}

		public override string GetTag () {
			return TAG;
		}
	}
}