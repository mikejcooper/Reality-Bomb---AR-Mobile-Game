using System;
using UnityEngine;
using Powerups;

namespace Abilities {
	public class AbilityRouter {

		public static string POWERUP_OBJ_NAME = "powerup";
		static string[] ALL_TAGS = new string[] { SandboxInkAbility.TAG, InkAbility.TAG, SpeedAbility.TAG, ShieldAbility.TAG, GrowAbility.TAG, ShrinkAbility.TAG };

		public static void RouteTag(String abilityTag, CarProperties carProperties, GameObject gameObject, BasePowerUpManager powerupManager, bool didTriggerPowerup, bool isLocalPlayer) {
			switch (abilityTag) {
			case SandboxInkAbility.TAG:
				Debug.Log("******** SANDBOX INKED! ********");
				SandboxInkAbility sandboxInkAbility = (SandboxInkAbility)gameObject.AddComponent(typeof(SandboxInkAbility));
				sandboxInkAbility.initialise(carProperties, (SandboxInkAbilityProperties) powerupManager.GetAbilityProperties(typeof(SandboxInkAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			case InkAbility.TAG:
				Debug.Log("******** INKED! ********");
				InkAbility inkAbility = (InkAbility)gameObject.AddComponent(typeof(InkAbility));
				inkAbility.initialise(carProperties, (InkAbilityProperties) powerupManager.GetAbilityProperties(typeof(InkAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			case SpeedAbility.TAG:
				Debug.Log("******** SPEED! ********");
				SpeedAbility speedAbility = (SpeedAbility)gameObject.AddComponent(typeof(SpeedAbility));
				speedAbility.initialise(carProperties, (SpeedAbilityProperties) powerupManager.GetAbilityProperties(typeof(SpeedAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			case ShieldAbility.TAG:
				Debug.Log("******** SHIELD! ********");
				ShieldAbility shieldAbility = (ShieldAbility)gameObject.AddComponent(typeof(ShieldAbility));
				shieldAbility.initialise(carProperties, (ShieldAbilityProperties) powerupManager.GetAbilityProperties(typeof(ShieldAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			case GrowAbility.TAG:
				Debug.Log("******** GROW! ********");
				GrowAbility growAbility = (GrowAbility)gameObject.AddComponent(typeof(GrowAbility));
				growAbility.initialise(carProperties, (GrowAbilityProperties) powerupManager.GetAbilityProperties(typeof(GrowAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			case ShrinkAbility.TAG:
				Debug.Log("******** Shrink! ********");
				ShrinkAbility shrinkAbility = (ShrinkAbility)gameObject.AddComponent(typeof(ShrinkAbility));
				shrinkAbility.initialise(carProperties, (ShrinkAbilityProperties) powerupManager.GetAbilityProperties(typeof(ShrinkAbility)), powerupManager.PlayerCanvas, didTriggerPowerup, isLocalPlayer, (AbilityCallbacks) powerupManager);
				break;
			}

		}

		public static bool IsAbilityObject (GameObject gameObject) {
			
//			foreach (var tag in ALL_TAGS) {
//				if (gameObject.name.StartsWith (tag)) {
//					return true;
//				}
//			}
//			return false;
			return gameObject.name.Equals (POWERUP_OBJ_NAME);
		}

//		public static string GetAbilityTag (GameObject gameObject) {
//			
//			foreach (var tag in ALL_TAGS) {
//				if (gameObject.name.StartsWith (tag)) {
//					return tag;
//				}
//			}
//			return null;
//		}
	}
}

