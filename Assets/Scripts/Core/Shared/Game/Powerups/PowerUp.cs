using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Abilities;

namespace Powerups {

	public class PowerUp : NetworkBehaviour {

		public PowerupDefinition PowerupDefinitionObj;
		public AbilityResources AbilityResources;

		void OnCollisionEnter (Collision collision) {
			if (collision.collider.tag == "TankTag") {
                Debug.Log("COLLIDED WITH TANK");

                /*
                if(!UnityEngine.Networking.NetworkServer.active) //Check we are in the sandbox
                {
                    var collidedObject = collision.gameObject;

                    if (PowerupDefinitionObj.Properties.GetType().IsAssignableFrom(typeof(SpeedAbilityProperties)))
                    {
                        SpeedAbility ability = (SpeedAbility)collidedObject.AddComponent(PowerupDefinitionObj.Ability);
                        ability.initialise(GetThisPlayerServerId(), (SpeedAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
                    }
                    else if (PowerupDefinitionObj.Properties.GetType().IsAssignableFrom(typeof(InkAbilityProperties)))
                    {
                        Debug.Log("INK ABILITY");
                        InkAbility ability = (InkAbility)collidedObject.AddComponent(PowerupDefinitionObj.Ability);
                        //ability.initialise(ClientSceneManager.Instance.GetThisPlayerData().ServerId, (InkAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
                        ability.initialise(GetThisPlayerServerId(), (InkAbilityProperties)PowerupDefinitionObj.Properties, AbilityResources);
                    }
                    else
                    {
                        Debug.LogError("unknown type: " + PowerupDefinitionObj.Properties.GetType().ToString());
                    }

                    Destroy(gameObject);
                }
                */				
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