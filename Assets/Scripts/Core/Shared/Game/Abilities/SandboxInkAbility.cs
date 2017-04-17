﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {
		
	[System.Serializable]
	public class SandboxInkAbilityProperties : BaseAbilityProperties {
		public Texture SplatterTexture;
	}
    
	public class SandboxInkAbility : BaseAbility<SandboxInkAbilityProperties> {

		public const string TAG = "sandboxinkability";

		protected GameObject _splatterObject;
        
		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
				_splatterObject = new GameObject ("Splatter");

				_splatterObject.transform.parent = canvas.transform;

				RawImage splatterImage = _splatterObject.AddComponent<RawImage> ();
				splatterImage.GetComponent<RectTransform> ().localPosition = canvas.gameObject.GetComponent<RectTransform> ().rect.center;
				splatterImage.GetComponent<RectTransform> ().localScale = 3.0f * Vector3.one;
				splatterImage.texture = _abilityProperties.SplatterTexture;

				// The following lines fade out the splatter effect over time
				splatterImage.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
				splatterImage.CrossFadeAlpha(0.0f,8.0f,false);
			}
		}

		override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
				Destroy (_splatterObject);
			}
		}

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				InkAbility.SetCarColor (properties);
			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (triggeredPowerup) {
				InkAbility.ResetCarColor (properties);
			}
		}

		public override string GetTag () {
			return TAG;
		}
     
	}
}