using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Abilities;

namespace Powerups {

	public class GamePowerUpManager: BasePowerUpManager {

		public SpeedAbilityProperties SpeedProperties;
		public InkAbilityProperties InkProperties;
		public ShieldAbilityProperties ShieldProperties;

		public GrowAbilityProperties GrowProperties;
		public ShrinkAbilityProperties ShrinkProperties;

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
					Debug.LogError ("Game Manager in GamePowerUpManager is Null");
				}
			}
				
			ClientScene.RegisterPrefab(PowerupPrefab);

//			  ClientScene.RegisterPrefab(ShieldProperties.PowerupPrefab);
//            ClientScene.RegisterPrefab(SpeedProperties.PowerupPrefab);
//            ClientScene.RegisterPrefab(InkProperties.PowerupPrefab);
        }

		protected override PowerupDefinition[] GetAvailablePowerups () {
            return new PowerupDefinition[] {
				new PowerupDefinition (typeof(SpeedAbility), SpeedAbility.TAG, SpeedProperties),
				new PowerupDefinition (typeof(InkAbility), InkAbility.TAG, InkProperties),
				new PowerupDefinition (typeof(ShieldAbility), ShieldAbility.TAG, ShieldProperties),

				new PowerupDefinition (typeof(GrowAbility), GrowAbility.TAG, GrowProperties),
				new PowerupDefinition (typeof(ShrinkAbility), ShrinkAbility.TAG, ShrinkProperties)
            };
        }


		protected override bool IsAllowedToSpawn(){
			return UnityEngine.Networking.NetworkServer.active;
		}

		protected override void OnPowerUpGenerated(GameObject powerUpObj) {
            if (isServer)
                NetworkServer.Spawn(powerUpObj);
		}

		private void LoadMesh(GameMapObjects gameMapObject){
			OnMeshReady (gameMapObject);
		}

	}
}