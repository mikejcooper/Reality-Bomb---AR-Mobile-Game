using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {
		
	[System.Serializable]
	public class InkAbilityProperties : BaseAbilityProperties {
		public Texture SplatterTexture;
	}

    public class InkAbilitySetup : BaseAbilitySetup
    {
        public GameObject _splatterObject;

        public InkAbilitySetup(Canvas canvas, InkAbilityProperties properties) : base(properties)
        {
            _splatterObject = new GameObject("Splatter");
            _splatterObject.transform.SetParent(canvas.transform, false);

            _splatterObject.transform.position = Vector3.zero;
            _splatterObject.transform.SetSiblingIndex(0);
            _splatterObject.transform.localPosition = Vector3.zero;

            for (int i = 0; i < 5; i++)
            {
                GameObject splat = new GameObject("Ink");
                RawImage splatterImage = splat.AddComponent<RawImage>();
                splatterImage.texture = ((InkAbilityProperties)AbilityProperties).SplatterTexture;
                splat.transform.SetParent(_splatterObject.transform, false);
                splat.SetActive(false);
            }
        }
    }

    public class InkAbility : BaseAbility<InkAbilitySetup> {

		public const string TAG = "inkability";
        
        
		override protected void OnApplyCanvasEffect (Canvas canvas, bool triggeredPowerup) {
            if (!triggeredPowerup)
            {
                Vector3[] positions = InkAbility.GenerateSplatters(canvas, 5);
                int index = 0;
                foreach (Transform child in _abilitySetup._splatterObject.transform)
                {
                    StartCoroutine(InkVisible(child.gameObject));
                    child.localPosition = positions[index];
                    child.Rotate(0, 0, Random.Range(0.0f, 360.0f));


                    float size = Random.Range(2.0f, 3.5f);
                    RawImage splatterImage = child.GetComponent<RawImage>();
                    splatterImage.GetComponent<RectTransform>().localScale = size * Vector3.one;
                    splatterImage.CrossFadeAlpha(0.0f, 0.0f, true);

                    index++;
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
            foreach (var splat in splatterImages)
            {
                splat.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                splat.CrossFadeAlpha(0.0f, 6.0f, false);
            }
        }

        override protected void OnRemoveCanvasEffect (Canvas canvas, bool triggeredPowerup) {
            if (!triggeredPowerup)
            {
                foreach (Transform child in _abilitySetup._splatterObject.transform)
                {
                    if (child.gameObject.GetComponent<CanvasRenderer>().GetAlpha() == 0.0f)
                        child.gameObject.SetActive(false);
                }
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

			foreach (var mat in materials) {
				Debug.Log (mat.name);
				if (mat.name.StartsWith ("Body")) {
					mat.color = Color.HSVToRGB(0f, 0f, 0f);
				}
			}

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