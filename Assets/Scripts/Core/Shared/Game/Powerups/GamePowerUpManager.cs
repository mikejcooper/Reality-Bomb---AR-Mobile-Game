﻿using System.Collections;
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
			if (isServer && _projectionAreaObj != null) {
				Vector3 p = powerUpObj.transform.position;
				powerUpObj.transform.position = new Vector3(p.x, p.y - (_yOffSet + 10.0f) , p.z);
				NetworkServer.Spawn (powerUpObj);
				_projectionAreaObj.GetComponent<ProjectObject> ().Launch (powerUpObj.transform, powerUpObj.transform.position);
			}
			
		}

		protected override void OnProjectionAreaGenerated(GameObject projectionAreaObj) {
			if (isServer) {
				NetworkServer.Spawn (projectionAreaObj);
				_projectionAreaObj = projectionAreaObj;
				_projectionAreaObj.GetComponent<ProjectObject> ().SetHeight (5.0f);
				_projectionAreaObj.GetComponent<ProjectObject> ().SetSpeed (0.5f);
			}
		}

		private void LoadMesh(GameMapObjects gameMapObject){
			OnMeshReady (gameMapObject);
		}
			
	}
}