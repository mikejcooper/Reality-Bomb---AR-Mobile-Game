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
        public BaseAbilitySetup Setup;

		public PowerupDefinition (Type ability, string tag, BaseAbilitySetup setup) {
			Ability = ability;
			Tag = tag;
            Setup = setup;
		}
	}

	public abstract class BasePowerUpManager : NetworkBehaviour, AbilityCallbacks {

		public Canvas PlayerCanvas;
		public GameObject ProjectionAreaPrefab;
		public GameObject PowerupPrefab;
		public float _yOffSet = 0.0f; 		// Temp made public 

		private GameMapObjects _meshObj;
        
		private bool _testingFlag; // False for Testing spawn frequency, True for actual spawn frequency.

		PowerupDefinition[] _availableAbilities;

        public SpawnPool PowerUpPool { get; protected set; }

        protected virtual void Start () {
			_availableAbilities = GetAvailablePowerups ();
			_testingFlag = false; // Set to false once testing is complete.                 
		}

        protected abstract PowerupDefinition[] GetAvailablePowerups ();

		public BaseAbilityProperties GetAbilityProperties(Type abilityType) {
			foreach (PowerupDefinition def in _availableAbilities) {
				if (def.Ability == abilityType) {
					return def.Setup.AbilityProperties;
				}
			}
			throw new KeyNotFoundException (string.Format("This powerup manager has no definition for an ability of type {0}", abilityType));
		}

        public BaseAbilitySetup GetAbilitySetup(Type abilityType)
        {
            foreach (PowerupDefinition def in _availableAbilities)
            {
                if (def.Ability == abilityType)
                {
                    return def.Setup;
                }
            }
            throw new KeyNotFoundException(string.Format("This powerup manager has no definition for an ability of type {0}", abilityType));
        }

        private void ValidateAbilities () {
			foreach (PowerupDefinition def in _availableAbilities) {
				if (def.Setup.AbilityProperties.CanvasSplash == null) {
					Debug.LogError ("CanvasSplash is undefined for some powerups");
				}

				if (def.Setup.AbilityProperties.Duration <= 0) {
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
				int rand;


				if (_testingFlag) {
					rand = Random.Range (0, 1);
				} else {
                    // Spawn frequency varies and gets lower as more powerups are spawned
					rand = Random.Range (0, (1 + PowerUpPool.CurrentNumber()));
				}

				// If generater produces the predetermined number from the range above, spawn a power up
				if (rand == 0) { 
					GenPowerUp ();
				}

				yield return new WaitForSeconds(5);
			}
		}
			
		private void GenProjectionArea () {
			GameObject projectionAreaObj = GameObject.Instantiate(ProjectionAreaPrefab);
			projectionAreaObj.transform.parent = GameObject.Find("Marker scene").transform;
			Vector3 startPosition = _meshObj.convexhullVertices[0];
			projectionAreaObj.transform.position = startPosition;
			//Scale convex hull points by 1.X%
//			List<Vector3> convexHull = GameUtils.MinimizeConvexHull(_meshObj.convexhullVertices, 1.2f);
//			projectionAreaObj.GetComponent<ProjectObject> ().SetPositions (convexHull);
			OnProjectionAreaGenerated (projectionAreaObj, _meshObj);
			if (projectionAreaObj.GetComponent<ProjectObject> ().onFinishedStartMovement()) {
				StartCoroutine (TryToSpawn());
			} else {
				projectionAreaObj.GetComponent<ProjectObject> ().OnFinshedStartMovementEvent += () => StartCoroutine (TryToSpawn());
			}
		
		}

		protected void OnMeshReady (GameMapObjects mesh) {
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
			Bounds bounds = _meshObj.ground.transform.GetComponent<MeshRenderer> ().bounds;
			_yOffSet = bounds.size.y / 2.0f;

			GenProjectionArea ();
//			StartCoroutine (TryToSpawn ());
		}


		// Generate a powerup once the decision to spawn one has been made
		private void GenPowerUp () {
            Vector3 position = GameUtils.FindSpawnLocationInsideConvexHull(_meshObj, 0.9f);
            position.y += (_yOffSet + 10.0f);

            GameObject powerUpObj = PowerUpPool.GetObject(position); //Will only provide us with a powerup if there is one available
            if (powerUpObj != null)
            {
                powerUpObj.transform.parent = GameObject.Find("Marker scene").transform;
                powerUpObj.name = "powerup";
//                powerUpObj.transform.localScale = Vector3.one;
				powerUpObj.GetComponent<Rigidbody> ().useGravity = false;
				powerUpObj.GetComponent<BoxCollider> ().enabled = false;

                if (NetworkServer.active)
                    NetworkServer.Spawn(powerUpObj, PowerUpPool.AssetId);
                
                OnPowerUpGenerated(powerUpObj);
            }               
            
		}
			
		public string GetPowerupType (GameObject powerupObj, bool hasBomb) {
			while (true) {
                var abilityTypeIndex = Random.Range (0, _availableAbilities.Length);
				string tag = _availableAbilities [abilityTypeIndex].Tag;
				if (hasBomb && tag.Equals (ShieldAbility.TAG)) {
					// if the player has the bomb, we don't want them to get a shield
					// todo: make this a generic property of abilities, as to whether
					// or not the car can have a bomb
					continue;
				}
				return tag;
			}
		}

		public virtual void OnAbilityStart (string abilityTag) {}
		public virtual void OnAbilityStop (string abilityTag) {}

		protected virtual bool IsAllowedToSpawn () { return false; }
		protected virtual void OnPowerUpGenerated(GameObject powerUpObj) {}
		protected virtual void OnProjectionAreaGenerated(GameObject projectionAreaObj, GameMapObjects meshObj) {}

	}

}