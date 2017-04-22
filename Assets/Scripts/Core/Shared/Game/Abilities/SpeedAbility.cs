using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {
	
	[System.Serializable]
	public class SpeedAbilityProperties : BaseAbilityProperties {
		public GameObject SparklesPrefab;
	}

	public class SpeedAbility : BaseAbility<SpeedAbilityProperties> {

		public const string TAG = "speed";
		public const int SPARKLES_LIFETIME_SECONDS = 5;
		private const float SPEED_FACTOR = 2.0f;

		private GameObject _sparklesObj;

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				_sparklesObj = GameObject.Instantiate (_abilityProperties.SparklesPrefab);
				_sparklesObj.transform.SetParent (properties.transform, false);
			
				properties.SpeedLimit *= SPEED_FACTOR;
				properties.Accel *= SPEED_FACTOR;
			
			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				var ps = _sparklesObj.GetComponentsInChildren<ParticleSystem>();
                foreach (var p in ps)
                {
                    var em = p.emission;
                    em.enabled = false;
                }

				// SpeedAbility will be destroyed as soon as this function exits,
				// so attach a delayed destroyer component to destroy the sparkles
				// after they've all gone.
				var destroyer = _sparklesObj.AddComponent<ObjectDestroyer> ();
				destroyer.DelayedDestroy (SPARKLES_LIFETIME_SECONDS);

				properties.SpeedLimit /= SPEED_FACTOR;
				properties.Accel /= SPEED_FACTOR;

			}
		}

		public override string GetTag () {
			return TAG;
		}
			
	}

}