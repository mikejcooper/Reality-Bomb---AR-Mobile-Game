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

		private GameObject _projectionAreaObj;

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
			ClientScene.RegisterPrefab(ProjectionAreaPrefab);
		}

		protected override PowerupDefinition[] GetAvailablePowerups () {
            InkAbilitySetup inkSetup = new InkAbilitySetup(PlayerCanvas, InkProperties);
            SpeedAbilitySetup speedSetup = new SpeedAbilitySetup(PlayerCanvas, SpeedProperties);
            ShieldAbilitySetup shieldSetup = new ShieldAbilitySetup(PlayerCanvas, ShieldProperties);
            GrowAbilitySetup growSetup = new GrowAbilitySetup(PlayerCanvas, GrowProperties);
            ShrinkAbilitySetup shrinkSetup = new ShrinkAbilitySetup(PlayerCanvas, ShrinkProperties);

            return new PowerupDefinition[] {
				new PowerupDefinition (typeof(SpeedAbility), SpeedAbility.TAG, speedSetup),
				new PowerupDefinition (typeof(InkAbility), InkAbility.TAG, inkSetup),
				new PowerupDefinition (typeof(ShieldAbility), ShieldAbility.TAG, shieldSetup),
				new PowerupDefinition (typeof(GrowAbility), GrowAbility.TAG, growSetup),
				new PowerupDefinition (typeof(ShrinkAbility), ShrinkAbility.TAG, shrinkSetup)
			};
		}


		protected override bool IsAllowedToSpawn(){
			return UnityEngine.Networking.NetworkServer.active;
		}

		protected override void OnPowerUpGenerated(GameObject powerUpObj) {
			if (isServer && _projectionAreaObj != null) {
				Vector3 p = powerUpObj.transform.position;
				Vector3 target = new Vector3(p.x, p.y - (_yOffSet + 10.0f) , p.z);
				// Move source to start position + some offset. 
				powerUpObj.transform.position = _projectionAreaObj.GetComponent<ProjectObject>().transform.position + new Vector3(0, 1.0f, 0);
				NetworkServer.Spawn (powerUpObj);
				_projectionAreaObj.GetComponent<ProjectObject> ().Launch (powerUpObj.transform, target);
			}
			
		}

		protected override void OnProjectionAreaGenerated(GameObject projectionAreaObj) {
			if (isServer) {
				NetworkServer.Spawn (projectionAreaObj);
				_projectionAreaObj = projectionAreaObj;
				_projectionAreaObj.GetComponent<ProjectObject> ().SetHeight (8.0f);
				_projectionAreaObj.GetComponent<ProjectObject> ().SetSpeed (0.5f);
			}
		}

		private void LoadMesh(GameMapObjects gameMapObject){
			OnMeshReady (gameMapObject);
		}
			
	}
}