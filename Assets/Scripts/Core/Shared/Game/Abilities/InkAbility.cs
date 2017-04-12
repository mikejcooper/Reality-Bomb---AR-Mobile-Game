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

		protected GameObject _splatterObject;
        
        override protected void OnApplyAbility(CarProperties properties, Canvas canvas) {
            
			_splatterObject = new GameObject ("Splatter");

			_splatterObject.transform.parent = canvas.transform;

			RawImage splatterImage = _splatterObject.AddComponent<RawImage> ();
			splatterImage.GetComponent<RectTransform> ().localPosition = canvas.gameObject.GetComponent<RectTransform> ().rect.center;
			splatterImage.GetComponent<RectTransform> ().localScale = 3.0f * Vector3.one;
			splatterImage.texture = _abilityProperties.SplatterTexture;

			// The following lines fade out the splatter effect over time
			splatterImage.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			splatterImage.CrossFadeAlpha(0.0f,8.0f,false);
            
            Debug.Log("Inking");
		}
        
		override protected void OnRemoveAbility(CarProperties properties, Canvas canvas) {
			Destroy (_splatterObject);
		}     
	}
}