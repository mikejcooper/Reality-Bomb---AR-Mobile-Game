using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NameStickExpander : MonoBehaviour {

	public GameObject TextObject;

	void Update () {
		transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0.8f + CalculateLengthOfMessage(), 0.5f);
	}

	float CalculateLengthOfMessage() {

		float totalLength = 0;

		UnityEngine.UI.Text text = TextObject.GetComponent<UnityEngine.UI.Text> ();
        
		CharacterInfo characterInfo = new CharacterInfo();

		char[] arr = text.text.ToCharArray();

		foreach(char c in arr)
		{
			text.font.GetCharacterInfo(c, out characterInfo, text.fontSize, text.fontStyle);  

			totalLength += characterInfo.advance;

		}

		return totalLength / 95.0f;
	}
}
