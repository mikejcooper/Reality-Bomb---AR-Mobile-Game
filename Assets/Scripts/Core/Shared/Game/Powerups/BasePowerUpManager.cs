using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Abilities;

namespace Powerups {


	public struct AbilityResources {
		public Canvas PlayerCanvas;
		public BasePowerUpManager manager;
	}

	public class PowerupDefinition {
		public Type Ability;
		public BaseAbilityProperties Properties;

		public PowerupDefinition (Type ability, BaseAbilityProperties properties) {
			Ability = ability;
			Properties = properties;
		}
	}

	public abstract class BasePowerUpManager : MonoBehaviour {

		public Canvas PlayerCanvas;

		private GameObject _meshObj;

		private float _yOffSet;
		private AbilityResources _abilityResources;

		PowerupDefinition[] _availableAbilities;

		protected virtual void Start () {
			_availableAbilities = GetAvailablePowerups ();
			_abilityResources.PlayerCanvas = PlayerCanvas;
			_abilityResources.manager = this;
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

		protected abstract PowerupDefinition[] GetAvailablePowerups ();

		IEnumerator TryToSpawn()
		{
			while(true) 
			{ 
				// Adjust Range Size to adjust spawn frequency
				int rand = Random.Range(0,5);

				// If generater produces the predetermined number from the range above, spawn a power up
				if (rand == 0|| rand == 1 || rand == 2/**/) { 
					GenPowerUp ();
				}

				yield return new WaitForSeconds(5);
			}
		}

		protected void OnMeshReady (GameObject mesh) {

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

			GameObject powerUpObj = GameObject.Instantiate (_availableAbilities [abilityTypeIndex].Properties.PowerupPrefab);
			powerUpObj.transform.parent = GameObject.Find("Marker scene").transform;
			powerUpObj.name = "powerup";

			powerUpObj.AddComponent<Rigidbody> ();
			powerUpObj.GetComponent<SphereCollider> ();

			Vector3 position = GameUtils.FindSpawnLocation (_meshObj);
			position.y += (_yOffSet + 10.0f);
			powerUpObj.transform.position = position;
			powerUpObj.transform.localScale = Vector3.one;

			PowerUp powerUp = powerUpObj.AddComponent<PowerUp> ();
			powerUp.PowerupDefinitionObj = _availableAbilities [abilityTypeIndex];
			powerUp.AbilityResources = _abilityResources;

			OnPowerUpGenerated (powerUpObj);
		}

		protected virtual void OnPowerUpGenerated(GameObject powerUpObj) {}

		public virtual void OnPowerUpStart<T>(BaseAbility<T> ability) where T:BaseAbilityProperties {}
		public virtual void OnPowerUpStop<T>(BaseAbility<T> ability) where T:BaseAbilityProperties {}

	}

}