using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Abilities;

namespace Powerups {

	public class GamePowerUpManager: BasePowerUpManager {

		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties InkProperties;
	

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
		}

		override protected PowerupDefinition[] GetAvailablePowerups () {
			return new PowerupDefinition[] { 
				new PowerupDefinition (typeof(SpeedAbility), SpeedProperties),
				new PowerupDefinition (typeof(SandboxInkAbility), InkProperties)
			};
		}

		public override void OnPowerUpStart<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'SBPUM': Speed boost activated");
				SpeedBoostActivatedEvent ();
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				InkSplatterActivatedEvent ();
				Debug.Log ("'SBPUM':Ink splatter activated");
			}
		}

		public override void OnPowerUpStop<T> (BaseAbility<T> ability) {
			if (ability.GetType ().IsAssignableFrom (typeof(SpeedAbility))) {
				Debug.Log ("'SBPUM':Speed boost deactivated");
			} else if (ability.GetType ().IsAssignableFrom (typeof(SandboxInkAbility))) {
				Debug.Log ("'SBPUM':Ink splatter deactivated");
			}
		}

		protected override bool IsAllowedToSpawn(){
			return UnityEngine.Networking.NetworkServer.active;
		}
//		public override void OnPowerUpGenerated(GameObject powerUpObj) {
//			powerUpObj.AddComponent<NetworkIdentity> ();
//		}

		private void LoadMesh(GameObject worldMesh){
			OnMeshReady (worldMesh);
		}
	}

}