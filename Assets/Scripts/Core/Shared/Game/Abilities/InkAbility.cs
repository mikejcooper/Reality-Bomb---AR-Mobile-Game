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
                _splatterObject = new GameObject("Splatter");
                _splatterObject.transform.SetParent(canvas.transform);

                _splatterObject.transform.position = Vector3.zero;
                _splatterObject.transform.SetSiblingIndex(0);
                _splatterObject.transform.localPosition = Vector3.zero;

                GenerateSplatters(canvas, 5);

                Invoke("FadeOut", 2);
            }
		}

        private void GenerateSplatters(Canvas canvas, int n)
        {
            Vector3[] positions = { new Vector3(-100,100,0),
                                    new Vector3(100,100,0),
                                    new Vector3(100,-100,0),
                                    new Vector3(-100,-100,0),
                                    new Vector3(0,0,0)};
            for (int i = 0; i < n; i++)
            {
                Vector3 rand_pos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), -1);

                StartCoroutine(CreateInk(positions[i] + rand_pos));
            }
        }

        IEnumerator CreateInk(Vector3 pos)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, 1.0f));

            GameObject splat = new GameObject("Ink");
            splat.transform.SetParent(_splatterObject.transform);
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
            foreach (var splat in splatterImages)
            {
                splat.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                splat.CrossFadeAlpha(0.0f, 6.0f, false);
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