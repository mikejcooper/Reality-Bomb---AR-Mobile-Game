using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class GrowAbilityProperties : BaseAbilityProperties {
//		public Texture GrowTexture;
	}

	public class GrowAbility : BaseAbility<GrowAbilityProperties> {

		public const string TAG = "growability";
		private const float SPEED_FACTOR = 1.5f;

		protected GameObject _growObject;

		private Vector3 _originalSize;
		private Vector3 _scale;


		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
				
			}
		}

		override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
				Destroy (_growObject);
			}
		}

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			gameObject.transform.localScale *= 3f;

			properties.MaxSpeed *= SPEED_FACTOR;
			properties.Acceleration *= SPEED_FACTOR;

		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			gameObject.transform.localScale /= 3f;

			properties.MaxSpeed /= SPEED_FACTOR;
			properties.Acceleration /= SPEED_FACTOR;
		}
			

		public override string GetTag () {
			return TAG;
		}

	}
}