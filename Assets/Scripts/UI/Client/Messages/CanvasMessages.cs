using UnityEngine;
using System.Collections;
using TMPro;


public class CanvasMessages : MonoBehaviour
{

	public GameObject textComponent;
			
	public void DisplayPowerUpMessage(string message){
		textComponent.transform.Find ("text").GetComponent<TextMeshProUGUI> ().text = message;
		textComponent.GetComponent<Animation>().Play();
	}
}

