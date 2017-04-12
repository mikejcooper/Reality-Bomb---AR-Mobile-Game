﻿using System.Collections;
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
		private GameObject _splashObject;

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
			Debug.Log (_thisPlayerServerId);
			Debug.Log (_calleeServerId);
			if (_thisPlayerServerId == _calleeServerId) {
				DisplaySplash ();
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

		private void DisplaySplash () {
			if (_abilityProperties.CanvasSplash != null) {
				
				_splashObject = GameObject.Instantiate (_abilityProperties.CanvasSplash);
				_splashObject.transform.parent = _abilityResources.PlayerCanvas.transform;
				_splashObject.transform.localScale = Vector3.one;
				_splashObject.transform.localPosition = Vector3.zero;
				Animation anim = _splashObject.GetComponent<Animation> ();
				if (anim != null) {
					anim.Play ();
					anim.wrapMode = WrapMode.Once;
					StartCoroutine (CheckForAnimationEnd(anim));
				} else {
					int secondsDelay = 2;
					Debug.LogWarning (string.Format ("No splash animation for {0}. Hiding in {1} seconds.", this.GetType ().Name));
					Invoke ("HideSplash", secondsDelay);
				}


			} else {
				Debug.LogWarning (string.Format ("No canvas splash for {0}", this.GetType ().Name));
			}

		}

		private IEnumerator CheckForAnimationEnd(Animation anim) {
			while (anim.isPlaying) {
				yield return new WaitForSeconds (0.5f);	
			} 

			HideSplash ();	
		}

		private void HideSplash () {
			Destroy (_splashObject);
		}

		private IEnumerator WaitForAnimation ( Animation animation )
		{
			do
			{
				yield return null;
			} while ( animation.isPlaying );
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