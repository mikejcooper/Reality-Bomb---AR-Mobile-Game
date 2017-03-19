using UnityEngine;
using System.Collections;
using TMPro;


public class CanvasMessages : MonoBehaviour
{

	public GameObject textComponent;


	// Use this for initialization
	void Start ()
	{
//		StartCoroutine(test());

	}
		
	public void DisplayPowerUpMessage(string message){
		textComponent.transform.Find ("text").GetComponent<TextMeshProUGUI> ().text = message;
		textComponent.GetComponent<Animation>().Play();
	}



	IEnumerator test()
	{
		DisplayPowerUpMessage ("hello");
		yield return new WaitForSeconds (5);
		DisplayPowerUpMessage ("hi");
	}
}

