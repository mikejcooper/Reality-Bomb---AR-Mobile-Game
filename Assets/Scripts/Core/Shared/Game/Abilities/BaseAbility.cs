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

	public interface AbilityCallbacks {
		void OnAbilityStart(string abilityTag);
		void OnAbilityStop(string abilityTag);
	}

	public abstract class BaseAbility<T> : MonoBehaviour where T:BaseAbilityProperties {

		private CarProperties _ownCarProperties;
		private Canvas _playerCanvas;
		private GameObject _splashObject;
		private bool _ownedByLocalPlayer;
		private AbilityCallbacks _callbacks;

		protected T _abilityProperties;

		public void initialise(CarProperties carProp, T abilityProp, Canvas canvas, bool ownedByLocalPlayer, AbilityCallbacks callbacks) {
			Debug.Log ("initialising ability");
            _ownCarProperties = carProp;
            _abilityProperties = abilityProp;
            _playerCanvas = canvas;
			_ownedByLocalPlayer = ownedByLocalPlayer;
			_callbacks = callbacks;
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
			_callbacks.OnAbilityStart (GetTag());
			if (_ownedByLocalPlayer) {
				DisplaySplash ();
            	OnApplyAbilitySelf(_ownCarProperties, _playerCanvas);
			} else {
				OnApplyAbilityOther(_ownCarProperties, _playerCanvas);
			}
		}

		public void StopAbility () {
			_callbacks.OnAbilityStop (GetTag());
			if (_ownedByLocalPlayer) {
				OnRemoveAbilitySelf (_ownCarProperties, _playerCanvas);
			} else {
				OnRemoveAbilityOther (_ownCarProperties, _playerCanvas);
			}
            Destroy (this);
		}

		private void DisplaySplash () {
			if (_abilityProperties.CanvasSplash != null) {
				
				_splashObject = GameObject.Instantiate (_abilityProperties.CanvasSplash);
				_splashObject.transform.parent = _playerCanvas.transform;
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

		public abstract string GetTag ();

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