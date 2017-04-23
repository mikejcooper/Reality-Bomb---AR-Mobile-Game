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
                _splatterObject.transform.SetParent(canvas.transform, false);

                _splatterObject.transform.position = Vector3.zero;
                _splatterObject.transform.SetSiblingIndex(0);
                _splatterObject.transform.localPosition = Vector3.zero;

				foreach (Vector3 position in GenerateSplatters(canvas, 5)) {
					StartCoroutine(CreateInk(position));
				}


                Invoke("FadeOut", 2);
            }
		}

		public static Vector3[] GenerateSplatters(Canvas canvas, int n)
        {
			float xOffset = canvas.pixelRect.width / 5.0f;
			float yOffset = canvas.pixelRect.height / 5.0f;
			Vector3[] positions = { new Vector3(-xOffset,yOffset,0),
									new Vector3(xOffset,yOffset,0),
									new Vector3(xOffset,-yOffset,0),
									new Vector3(-xOffset,-yOffset,0),
                                    new Vector3(0,0,0)};
			Vector3[] randPositions = new Vector3[n];
			float xWriggle = xOffset / 2.0f;
			float yWriggle = yOffset / 2.0f;
            for (int i = 0; i < n; i++)
            {
				randPositions[i] = positions[i] + new Vector3(Random.Range(-xWriggle, yWriggle), Random.Range(-xWriggle, yWriggle), -1);
            }

			return randPositions;
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