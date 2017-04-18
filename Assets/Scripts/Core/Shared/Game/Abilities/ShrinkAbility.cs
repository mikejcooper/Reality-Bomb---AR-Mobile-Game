using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

	[System.Serializable]
	public class ShrinkAbilityProperties : BaseAbilityProperties {
		//		public Texture GrowTexture;
	}

	public class ShrinkAbility : BaseAbility<ShrinkAbilityProperties> {

		public const string TAG = "shrinkability";
		private const float SPEED_FACTOR = 1.5f;

		protected GameObject _shrinkObject;

		private Vector3 _originalSize;
		private Vector3 _scale;
		private bool _maxShrink;


		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {

			}
		}

		override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
				Destroy (_shrinkObject);
			}
		}

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			_maxShrink = false;

			if (gameObject.transform.localScale.x > 0.25f) {
				gameObject.transform.localScale /= 2f;

				properties.MaxSpeed /= SPEED_FACTOR;
				properties.Acceleration /= SPEED_FACTOR;
			} else {
				_maxShrink = true;
			}


		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (!_maxShrink) {
				gameObject.transform.localScale *= 2f;

				properties.MaxSpeed *= SPEED_FACTOR;
				properties.Acceleration *= SPEED_FACTOR;
			}
		}


		public override string GetTag () {
			return TAG;
		}

	}
}