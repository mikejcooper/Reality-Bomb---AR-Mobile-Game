using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {
		
	[System.Serializable]
	public class InkAbilityProperties : BaseAbilityProperties {
		public Texture SplatterTexture;
	}
    
	public class InkAbility : BaseAbility<InkAbilityProperties> {

		public const string TAG = "inkability";

		private GameObject _splatterObject;
        
		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (!triggeredPowerup) {
				_splatterObject = new GameObject ("Splatter");

				_splatterObject.transform.parent = canvas.transform;

				RawImage splatterImage = _splatterObject.AddComponent<RawImage> ();
				splatterImage.GetComponent<RectTransform> ().localPosition = canvas.gameObject.GetComponent<RectTransform> ().rect.center;
				splatterImage.GetComponent<RectTransform> ().localScale = 3.0f * Vector3.one;
				splatterImage.texture = _abilityProperties.SplatterTexture;

				// The following lines fade out the splatter effect over time
				splatterImage.GetComponent<CanvasRenderer> ().SetAlpha (1.0f);
				splatterImage.CrossFadeAlpha (0.0f, 8.0f, false);
			}
		}

		override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (!triggeredPowerup) {
				Destroy (_splatterObject);
			}
		}

		protected override void OnApplyCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (!triggeredPowerup) {
				SetCarColor (properties);
			}
		}

		protected override void OnRemoveCarEffect (CarProperties properties, bool triggeredPowerup) {
			if (!triggeredPowerup) {
				ResetCarColor (properties);
			}
		}

		// alters the value property of the car materials' HSV
		public static void SetCarColor (CarProperties properties) {
			Material[] materials = properties.transform.FindChild("Car_Model").GetComponent<MeshRenderer> ().materials;

			materials [1].color = Color.HSVToRGB(0f, 0f, 0f); // Side glow
			materials [2].color = Color.HSVToRGB(0f, 0f, 0f); // Blades
			materials [3].color = Color.HSVToRGB(0f, 0f, 0f); // Body
		}

		public static void ResetCarColor (CarProperties properties) {
			Material[] materials = properties.transform.FindChild("Car_Model").GetComponent<MeshRenderer> ().materials;

			GameUtils.SetCarMaterialColoursFromHue (materials, properties.OriginalHue);
		}



		public override string GetTag () {
			return TAG;
		}
     
	}
}