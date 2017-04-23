using System.Collections;
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
                _splatterObject = new GameObject("Splatter");
                _splatterObject.transform.SetParent(canvas.transform, false);

                _splatterObject.transform.position = Vector3.zero;
                _splatterObject.transform.SetSiblingIndex(0);
                _splatterObject.transform.localPosition = Vector3.zero;

				foreach (Vector3 position in InkAbility.GenerateSplatters(canvas, 5)) {
					StartCoroutine(CreateInk(position));
				}

                Invoke("FadeOut", 2);
            }
		}

        IEnumerator CreateInk(Vector3 pos)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, 1.0f));

            GameObject splat = new GameObject("Ink");
            splat.transform.SetParent(_splatterObject.transform, false);
            splat.transform.localPosition = pos;
            splat.transform.Rotate(0, 0, Random.Range(0.0f, 360.0f));

            RawImage splatterImage = splat.AddComponent<RawImage>();
            float size = Random.Range(2.0f, 3.5f);
            splatterImage.GetComponent<RectTransform>().localScale = size * Vector3.one;
            splatterImage.texture = _abilityProperties.SplatterTexture;
        }

        private void FadeOut()
        {
            RawImage[] splatterImages = _splatterObject.GetComponentsInChildren<RawImage>();
            // The following lines fade out the splatter effect over time
            foreach(var splat in splatterImages)
            {
                splat.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                splat.CrossFadeAlpha(0.0f, 6.0f, false);
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