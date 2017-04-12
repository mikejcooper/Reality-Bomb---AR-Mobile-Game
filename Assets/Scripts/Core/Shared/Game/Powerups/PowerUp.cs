using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Abilities;

namespace Powerups {

	public class PowerUp : MonoBehaviour {

		public PowerupDefinition PowerupDefinitionObj;
		public AbilityResources AbilityResources;

		void OnCollisionEnter (Collision collision) {
			if (collision.collider.tag == "TankTag") {
				var collidedObject = collision.gameObject;

				if (PowerupDefinitionObj.Properties.GetType ().IsAssignableFrom (typeof(SpeedAbilityProperties))) {
					SpeedAbility ability = (SpeedAbility)collidedObject.AddComponent (PowerupDefinitionObj.Ability);
					ability.initialise (GetThisPlayerServerId (), (SpeedAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
				} else if (PowerupDefinitionObj.Properties.GetType ().IsAssignableFrom (typeof(SandboxInkAbilityProperties))) {
					SandboxInkAbility ability = (SandboxInkAbility)collidedObject.AddComponent (PowerupDefinitionObj.Ability);
//					ability.initialise (ClientSceneManager.Instance.GetThisPlayerData ().ServerId, (SandboxInkAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
					ability.initialise (GetThisPlayerServerId (), (SandboxInkAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
				} else {
					Debug.LogError ("unknown type: " + PowerupDefinitionObj.Properties.GetType ().ToString ());
				}

				Destroy (gameObject);
			}
		}

		private int GetThisPlayerServerId () {
			if (ClientSceneManager.Instance != null) {
				Debug.Log (ClientSceneManager.Instance.GetThisPlayerData ().ServerId);
				return ClientSceneManager.Instance.GetThisPlayerData ().ServerId;
			} else {
				return 0;
			}
		}

	}

}