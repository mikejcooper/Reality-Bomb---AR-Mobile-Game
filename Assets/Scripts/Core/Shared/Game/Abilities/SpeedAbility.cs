using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {
	
	[System.Serializable]
	public class SpeedAbilityProperties : BaseAbilityProperties {}

	public class SpeedAbility : BaseAbility<SpeedAbilityProperties> {

		override protected void OnApplyAbility (CarController car, Canvas canvas) {
			car.CarProperties.MaxSpeed *= 2.0f;
			car.CarProperties.Acceleration *= 2.0f;
		}

		override protected void OnRemoveAbility (CarController car, Canvas canvas) {
			car.CarProperties.MaxSpeed /= 2.0f;
			car.CarProperties.Acceleration /= 2.0f;
		}
			
	}

}