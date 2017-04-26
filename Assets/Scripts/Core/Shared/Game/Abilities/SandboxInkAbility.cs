using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {
		
	[System.Serializable]
	public class SandboxInkAbilityProperties : BaseAbilityProperties {
		public Texture SplatterTexture;
	}

    public class SandboxInkAbilitySetup : BaseAbilitySetup
    {        
        public GameObject _splatterObject;        
        
        public SandboxInkAbilitySetup(Canvas canvas, SandboxInkAbilityProperties properties) : base(properties)
        {
            _splatterObject = new GameObject("Splatter");
            _splatterObject.transform.SetParent(canvas.transform, false);

            _splatterObject.transform.position = Vector3.zero;
            _splatterObject.transform.SetSiblingIndex(0);
            _splatterObject.transform.localPosition = Vector3.zero;

            for (int i= 0; i<5; i++)
            {
                GameObject splat = new GameObject("Ink");
                RawImage splatterImage = splat.AddComponent<RawImage>();
                splatterImage.texture = ((SandboxInkAbilityProperties) AbilityProperties).SplatterTexture;
                splat.transform.SetParent(_splatterObject.transform, false);
                splat.SetActive(false);
            }
        }
    }

    public class SandboxInkAbility : BaseAbility<SandboxInkAbilitySetup> {

		public const string TAG = "sandboxinkability";
        
        
		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
                Vector3[] positions = InkAbility.GenerateSplatters(canvas, 5);
                int index = 0;
                foreach (Transform child in _abilitySetup._splatterObject.transform)
                {
                    StartCoroutine(InkVisible(child.gameObject));
                    //child.gameObject.SetActive(true);
                    child.localPosition = positions[index];
                    child.Rotate(0, 0, Random.Range(0.0f, 360.0f));

                    
                    float size = Random.Range(2.0f, 3.5f);
                    RawImage splatterImage = child.GetComponent<RawImage>();
                    splatterImage.GetComponent<RectTransform>().localScale = size * Vector3.one;
                    splatterImage.CrossFadeAlpha(0.0f, 0.0f, true);

                    //(InkVisible(child.gameObject));

                    index++;
                }

                Invoke("FadeOut", 2);
            }
		}

        
        IEnumerator InkVisible(GameObject obj)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, 1.0f));
            obj.GetComponent<RawImage>().CrossFadeAlpha(1.0f, 0.0f, true);
            obj.SetActive(true);
        }

        private void FadeOut()
        {
            RawImage[] splatterImages = _abilitySetup._splatterObject.GetComponentsInChildren<RawImage>();
            // The following lines fade out the splatter effect over time
            foreach(var splat in splatterImages)
            {
                splat.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                splat.CrossFadeAlpha(0.0f, 6.0f, false);
            }
        }

        override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
			if (triggeredPowerup) {
                foreach (Transform child in _abilitySetup._splatterObject.transform)
                {
                    if (child.gameObject.GetComponent<CanvasRenderer>().GetAlpha() == 0.0f)
                        child.gameObject.SetActive(false);
                }
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