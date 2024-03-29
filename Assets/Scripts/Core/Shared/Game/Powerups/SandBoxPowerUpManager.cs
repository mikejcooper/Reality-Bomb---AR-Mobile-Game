﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace Powerups {

	public class SandBoxPowerUpManager : BasePowerUpManager {

		public SpeedAbilityProperties SpeedProperties;
		public SandboxInkAbilityProperties SandboxInkProperties;
		public ShieldAbilityProperties ShieldProperties;
		public GrowAbilityProperties GrowProperties;
		public ShrinkAbilityProperties ShrinkProperties;

		public delegate void OnSpeedBoostActivated();
		public delegate void OnInkSplatterActivated();
		public delegate void OnShieldActivated();
		public delegate void OnGrowActivated();
		public delegate void OnShrinkActivated();
		public event OnSpeedBoostActivated SpeedBoostActivatedEvent = delegate {};
		public event OnInkSplatterActivated InkSplatterActivatedEvent = delegate {};
		public event OnShieldActivated ShieldActivatedEvent = delegate {};
		public event OnGrowActivated GrowActivatedEvent = delegate {};
		public event OnShrinkActivated ShrinkActivatedEvent = delegate{};

		public GameObject PlaneObject;

		private GameObject _projectionAreaObj;

		protected override void Start () {
			base.Start ();
			GameMapObjects gameMapObjects = new GameMapObjects (PlaneObject, PlaneObject, GetBoundingBox (PlaneObject.GetComponent<MeshFilter> ().mesh, PlaneObject));
			OnMeshReady (gameMapObjects);
            PowerUpPool = new SpawnPool(PowerupPrefab, 4, false);
        }
        
        private List<Vector3> GetBoundingBox(Mesh mesh, GameObject _planeObject){
			List<Vector3> result = new List<Vector3> ();
			Vector3 scale = _planeObject.transform.localScale;
			Vector3 min = mesh.bounds.min;
			Vector3 max = mesh.bounds.max;
			result.Add (new Vector3(max.x * scale.x, 0, min.z * scale.z));
			result.Add (new Vector3(max.x * scale.x, 0, max.z * scale.z));
			result.Add (new Vector3(min.x * scale.x, 0, max.z * scale.z));
			result.Add (new Vector3(min.x * scale.x, 0, min.z * scale.z));
			return result;
		}

		protected override PowerupDefinition[] GetAvailablePowerups () {

            SandboxInkAbilitySetup sandboxInkSetup = new SandboxInkAbilitySetup(PlayerCanvas, SandboxInkProperties);
            SpeedAbilitySetup speedSetup = new SpeedAbilitySetup(PlayerCanvas, SpeedProperties);
            ShieldAbilitySetup shieldSetup = new ShieldAbilitySetup(PlayerCanvas, ShieldProperties);
            GrowAbilitySetup growSetup = new GrowAbilitySetup(PlayerCanvas, GrowProperties);
            ShrinkAbilitySetup shrinkSetup = new ShrinkAbilitySetup(PlayerCanvas, ShrinkProperties);

            return new PowerupDefinition[] {
                new PowerupDefinition (typeof(SpeedAbility), SpeedAbility.TAG, speedSetup),
                new PowerupDefinition (typeof(SandboxInkAbility), SandboxInkAbility.TAG, sandboxInkSetup),
                new PowerupDefinition (typeof(ShieldAbility), ShieldAbility.TAG, shieldSetup),
                new PowerupDefinition (typeof(GrowAbility), GrowAbility.TAG, growSetup),
                new PowerupDefinition (typeof(ShrinkAbility), ShrinkAbility.TAG, shrinkSetup)
            };
		}

		public override void OnAbilityStart (string abilityTag) {
			base.OnAbilityStart(abilityTag);
			switch (abilityTag) {
			case SandboxInkAbility.TAG:
				InkSplatterActivatedEvent ();
				break;
			case SpeedAbility.TAG:
				SpeedBoostActivatedEvent ();
				break;
			case ShieldAbility.TAG:
				ShieldActivatedEvent ();
				break;
			case GrowAbility.TAG:
				GrowActivatedEvent ();
				break;
			case ShrinkAbility.TAG:
				ShrinkActivatedEvent ();
				break;
			}
		}

		protected override bool IsAllowedToSpawn(){
			return true;
		}

		protected override void OnPowerUpGenerated(GameObject powerUpObj) {
			if (_projectionAreaObj != null) {
				Vector3 p = powerUpObj.transform.position;
				Vector3 target = new Vector3(p.x, p.y - (_yOffSet + 10.0f) , p.z);
				// Move source to start position + some offset. 
				powerUpObj.transform.position = _projectionAreaObj.GetComponent<ProjectObject>().transform.position + new Vector3(0, 1.0f, 0);
				_projectionAreaObj.GetComponent<ProjectObject> ().Launch (powerUpObj.transform, target);
			}
		}

		protected override void OnProjectionAreaGenerated(GameObject projectionAreaObj, GameMapObjects meshObj) {
			List<Vector3> convexHull = GameUtils.MinimizeConvexHull(meshObj.convexhullVertices, 1.2f);
			_projectionAreaObj = projectionAreaObj;
			projectionAreaObj.GetComponent<ProjectObject> ().SetPositions (convexHull);
			projectionAreaObj.GetComponent<ProjectObject> ().SetHeight (4.0f);
			projectionAreaObj.GetComponent<ProjectObject> ().SetSpeed (0.5f);
		}
	}
}