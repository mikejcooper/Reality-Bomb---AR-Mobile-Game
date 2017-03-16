using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Powerups;

namespace Abilities {

	[System.Serializable]
	public abstract class BaseAbilityProperties {
		public GameObject PowerupPrefab;
		public GameObject CanvasSplash;
		public int Duration;
	}

	public abstract class BaseAbility<T> : MonoBehaviour where T:BaseAbilityProperties {
		
		private int _calleeServerId;
		private int _thisPlayerServerId;
		private CarProperties _ownCarProperties;
		private AbilityResources _abilityResources;

		protected T _abilityProperties;

		public void initialise(int calleeServerId, T properties, AbilityResources abilityResources) {
			_calleeServerId = calleeServerId;
			_abilityProperties = properties;
			_abilityResources = abilityResources;
			if (!UnityEngine.Networking.NetworkServer.active) {
				_thisPlayerServerId = GetThisPlayerServerId();
				var ownCarPropertiesArray = GameObject.FindObjectsOfType<CarProperties> ().Where (IsOwnCarProperties);
				Debug.Assert (ownCarPropertiesArray.Count() > 0, "there should be a CarProperties this client owns");
				Debug.Assert (ownCarPropertiesArray.Count() == 1, "there should only be one CarProperties this client owns");
				_ownCarProperties = ownCarPropertiesArray.First ();
			}

		}

		private int GetThisPlayerServerId () {
			if (ClientSceneManager.Instance != null) {
				return ClientSceneManager.Instance.GetThisPlayerData ().ServerId;
			} else {
				return 0;
			}
		}

		// start is called on the frame when a script is enabled just 
		// before any of the update methods is called the first time.
		void Start () {
			StartAbility ();
			Invoke ("StopAbility", _abilityProperties.Duration);
		}

		public void StartAbility () {
			_abilityResources.manager.OnPowerUpStart (this);
			if (_thisPlayerServerId == _calleeServerId) {
				OnApplyAbilitySelf (_ownCarProperties, _abilityResources.PlayerCanvas);
			} else {
				OnApplyAbilityOther (_ownCarProperties, _abilityResources.PlayerCanvas);
			}
		}

		public void StopAbility () {
			_abilityResources.manager.OnPowerUpStop (this);
			if (_thisPlayerServerId == _calleeServerId) {
				OnRemoveAbilitySelf (_ownCarProperties, _abilityResources.PlayerCanvas);
			} else {
				OnRemoveAbilityOther (_ownCarProperties, _abilityResources.PlayerCanvas);
			}
			Destroy (this);
		}

		private bool IsOwnCarProperties(CarProperties properties) {
			var identity = properties.GetComponentInParent<NetworkIdentity> ();
			return identity == null || identity.clientAuthorityOwner == null || identity.hasAuthority;

		}

		// Called on client that triggered this ability.
		// Apply things that should affect the car that triggered the event,
		// like speed boost.
		protected virtual void OnApplyAbilitySelf(CarProperties properties, Canvas canvas) {}

		// Called on individual clients that didn't trigger this ability
		// Apply things that should affect all clients that didn't trigger,
		// the event, like an ink splatter.
		protected virtual void OnApplyAbilityOther(CarProperties properties, Canvas canvas) {}

		// Called on client that triggered this ability
		protected virtual void OnRemoveAbilitySelf(CarProperties properties, Canvas canvas) {}

		// Called on individual clients that didn't trigger this ability
		protected virtual void OnRemoveAbilityOther(CarProperties properties, Canvas canvas) {}
	}
}