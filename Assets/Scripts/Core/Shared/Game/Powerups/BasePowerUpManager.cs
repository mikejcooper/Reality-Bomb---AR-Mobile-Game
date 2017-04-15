using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using Abilities;

namespace Powerups { 

	public class PowerupDefinition {
		public Type Ability;
		public String Tag;
		public BaseAbilityProperties Properties;

		public PowerupDefinition (Type ability, string tag, BaseAbilityProperties properties) {
			Ability = ability;
			Tag = tag;
			Properties = properties;
		}
	}

	public abstract class BasePowerUpManager : NetworkBehaviour, AbilityCallbacks {

		public Canvas PlayerCanvas;

        private GameObject _meshObj;
		private float _yOffSet;

		PowerupDefinition[] _availableAbilities;
        
        protected virtual void Start () {
			_availableAbilities = GetAvailablePowerups ();
		}

		protected abstract PowerupDefinition[] GetAvailablePowerups ();

		public BaseAbilityProperties GetAbilityProperties(Type abilityType) {
			foreach (PowerupDefinition def in GetAvailablePowerups()) {
				if (def.Ability == abilityType) {
					return def.Properties;
				}
			}
			throw new KeyNotFoundException (string.Format("This powerup manager has no definition for an ability of type {0}", abilityType));
		}

        private void ValidateAbilities () {
			foreach (PowerupDefinition def in _availableAbilities) {
				if (def.Properties.CanvasSplash == null) {
					Debug.LogError ("CanvasSplash is undefined for some powerups");
				}

				if (def.Properties.PowerupPrefab == null) {
					Debug.LogError ("PowerupPrefab is undefined for some powerups");
				}

				if (def.Properties.Duration <= 0) {
					Debug.LogError ("Duration is invalid for some powerups");
				}
					
			}
			Debug.Log(string.Format("{0} powerups ready to spawn", _availableAbilities.Length));
		}
        

		IEnumerator TryToSpawn()
		{
			Debug.Log ("Trying to spawn powerup");
			while(true) 
			{ 
				// Adjust Range Size to adjust spawn frequency
				int rand = Random.Range(0,3);

				// If generater produces the predetermined number from the range above, spawn a power up
				if (rand == 0) { 
					GenPowerUp ();
				}

				yield return new WaitForSeconds(5);
			}
		}

		protected void OnMeshReady (GameObject mesh) {
			if (!IsAllowedToSpawn ()) {
				Debug.Log ("This PowerUpManager is not allowed to spawn");
				return;
			}

			if (mesh == null) {
				Debug.LogError ("OnMeshReady: mesh is null!");
				return;
			}

			_meshObj = mesh;
			Debug.Log ("OnMeshReady");
			Bounds bounds = _meshObj.transform.GetComponent<MeshRenderer> ().bounds;
			_yOffSet = bounds.size.y / 2.0f;

			StartCoroutine (TryToSpawn());
		}


		// Generate a powerup once the decision to spawn one has been made
		private void GenPowerUp () {
            var abilityTypeIndex = Random.Range(0,_availableAbilities.Length);
            GameObject powerUpObj = GameObject.Instantiate (_availableAbilities [abilityTypeIndex].Properties.PowerupPrefab) as GameObject;
			powerUpObj.transform.parent = GameObject.Find("Marker scene").transform;
			powerUpObj.GetComponent<SphereCollider> ();
			powerUpObj.name = _availableAbilities [abilityTypeIndex].Tag;

            Vector3 position = GameUtils.FindSpawnLocation (_meshObj);
			position.y += (_yOffSet + 10.0f);
			powerUpObj.transform.position = position;
			powerUpObj.transform.localScale = Vector3.one;

			OnPowerUpGenerated (powerUpObj);
		}

		public virtual void OnAbilityStart (string abilityTag) {}
		public virtual void OnAbilityStop (string abilityTag) {}

        protected virtual bool IsAllowedToSpawn () { return false; }
		protected virtual void OnPowerUpGenerated(GameObject powerUpObj) {}

	}

}