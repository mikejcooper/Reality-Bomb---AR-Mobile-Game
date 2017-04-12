using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Abilities;

namespace Powerups {

	public class GamePowerUpManager: BasePowerUpManager {

		public SpeedAbilityProperties SpeedProperties;
		public InkAbilityProperties InkProperties;
	

		// Events
		public delegate void OnSpeedBoostActivated ();
		public delegate void OnInkSplatterActivated ();
		public static event OnSpeedBoostActivated SpeedBoostActivatedEvent;		
		public static event OnInkSplatterActivated InkSplatterActivatedEvent;

		protected override void Start () {
			base.Start ();
			if (IsAllowedToSpawn ()) {
				if (GameObject.FindObjectOfType<GameManager> () != null) {
					if (GameObject.FindObjectOfType<GameManager> ().WorldMesh != null) {
						LoadMesh (GameObject.FindObjectOfType<GameManager> ().WorldMesh);
					} else {
						GameObject.FindObjectOfType<GameManager> ().OnWorldMeshAvailableEvent += LoadMesh;
					}
				} else {
					Debug.LogError ("Game Manager in GamePowerUpMaager is Null");
				}
			}

            ClientScene.RegisterPrefab(SpeedProperties.PowerupPrefab);
            ClientScene.RegisterPrefab(InkProperties.PowerupPrefab);
        }

		override protected PowerupDefinition[] GetAvailablePowerups () {
			return new PowerupDefinition[] { 
				new PowerupDefinition (typeof(SpeedAbility), SpeedProperties),
				new PowerupDefinition (typeof(InkAbility), InkProperties)
			};
		}

		public override void OnPowerUpStart<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'GPUM': Speed boost activated");
				SpeedBoostActivatedEvent ();
			} else if (ability.GetType ().IsAssignableFrom (typeof(InkAbility))) {
				InkSplatterActivatedEvent ();
				Debug.Log ("'GPUM':Ink splatter activated");
			}
		}

		public override void OnPowerUpStop<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'GPUM':Speed boost deactivated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(InkAbility))) {
				Debug.Log ("'GPUM':Ink splatter deactivated");
			}
		}

		protected override bool IsAllowedToSpawn(){
			return UnityEngine.Networking.NetworkServer.active;
		}

		protected override void OnPowerUpGenerated(GameObject powerUpObj) {
            if (isServer)
                NetworkServer.Spawn(powerUpObj);
		}

		private void LoadMesh(GameObject worldMesh){
			OnMeshReady (worldMesh);
		}



	}

}