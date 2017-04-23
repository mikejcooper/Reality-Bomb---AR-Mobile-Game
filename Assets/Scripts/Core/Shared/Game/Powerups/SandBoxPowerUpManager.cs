using System.Collections;
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
			return new PowerupDefinition[] {
				new PowerupDefinition (typeof(SpeedAbility), SpeedAbility.TAG, SpeedProperties),
				new PowerupDefinition (typeof(SandboxInkAbility), SandboxInkAbility.TAG, SandboxInkProperties),
				new PowerupDefinition (typeof(ShieldAbility), ShieldAbility.TAG, ShieldProperties),
				new PowerupDefinition (typeof(GrowAbility), GrowAbility.TAG, GrowProperties),
				new PowerupDefinition (typeof(ShrinkAbility), ShrinkAbility.TAG, ShrinkProperties)
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
				powerUpObj.transform.position = new Vector3(p.x, p.y - (_yOffSet + 10.0f) , p.z);
				_projectionAreaObj.GetComponent<ProjectObject> ().Launch (powerUpObj.transform, powerUpObj.transform.position);
			}
		}

		protected override void OnProjectionAreaGenerated(GameObject projectionAreaObj) {
			_projectionAreaObj = projectionAreaObj;
			_projectionAreaObj.GetComponent<ProjectObject> ().SetHeight (5.0f);
			_projectionAreaObj.GetComponent<ProjectObject> ().SetSpeed (0.5f);
		}


	}

}