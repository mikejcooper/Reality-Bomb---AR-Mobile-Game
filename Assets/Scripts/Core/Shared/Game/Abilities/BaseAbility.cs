using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Powerups;

namespace Abilities {

	[System.Serializable]
	public abstract class BaseAbilityProperties {
		public GameObject CanvasSplash;
		public int Duration;
	}

    //Used for any object initialisation (pooling)
    public abstract class BaseAbilitySetup
    {
        public BaseAbilityProperties AbilityProperties;
        //Creates all objects in advance
        public BaseAbilitySetup(BaseAbilityProperties baseProps)
        {
            AbilityProperties = baseProps;
        }
    }

	public interface AbilityCallbacks {
		void OnAbilityStart(string abilityTag);
		void OnAbilityStop(string abilityTag);
	}

	public abstract class BaseAbility<T> : MonoBehaviour where T:BaseAbilitySetup {

		private CarProperties _ownCarProperties;
		private Canvas _playerCanvas;
		private GameObject _splashObject;
		private bool _didTriggerPowerup;
		private bool _isLocalAuthority;
		private AbilityCallbacks _callbacks;
        
        protected T _abilitySetup;

		public void initialise(CarProperties carProp, T abilitySetup, Canvas canvas, bool didTriggerPowerup, bool isLocalAuthority, AbilityCallbacks callbacks) {
			Debug.Log ("initialising ability");
            _ownCarProperties = carProp;
            _abilitySetup = abilitySetup;
            _playerCanvas = canvas;
			_didTriggerPowerup = didTriggerPowerup;
			_isLocalAuthority = isLocalAuthority;
			_callbacks = callbacks;
		}

		// start is called on the frame when a script is enabled just 
		// before any of the update methods is called the first time.
		void Start () {
			StartAbility ();
			Invoke ("StopAbility", _abilitySetup.AbilityProperties.Duration);
		}

		public void StartAbility () {
			_callbacks.OnAbilityStart (GetTag());

			OnApplyCarEffect (_ownCarProperties, _didTriggerPowerup);
			if (_isLocalAuthority) {
				OnApplyCanvasEffect (_playerCanvas, _didTriggerPowerup);
				if (_didTriggerPowerup) {
					DisplaySplash ();
				}
			}

		}

		public void StopAbility () {
			_callbacks.OnAbilityStop (GetTag());
			OnRemoveCarEffect (_ownCarProperties, _didTriggerPowerup);
			if (_isLocalAuthority) {
				OnRemoveCanvasEffect (_playerCanvas, _didTriggerPowerup);
			}
            Destroy (this);
		}

		private void DisplaySplash () {
			if (_abilitySetup.AbilityProperties.CanvasSplash != null) {
				
				_splashObject = GameObject.Instantiate (_abilitySetup.AbilityProperties.CanvasSplash);
				_splashObject.transform.SetParent(_playerCanvas.transform, false);
				_splashObject.transform.localScale = Vector3.one;
//				_splashObject.transform.localPosition = Vector3.zero;
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

		// these are called individually for every car in the scene
		protected virtual void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {}
		protected virtual void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {}

		protected virtual void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {}
		protected virtual void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {}


	}
}