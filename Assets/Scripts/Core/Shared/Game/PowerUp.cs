using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour {

	public Canvas PlayerCanvas;
	public Texture SplatterTex;

	// will store the type of power up (boost, invinsible, invisible, etc)
	private int P_Type;
	private GameObject _collidedObject;
	private GameObject _splatterObject;

	private string _gui_Txt;
	private string _splatter_Txt = "You've Activated the Splatter Power Up!\nThis splatters ink on your opponents' screens\n as shown above making it harder for them to see!";
	private string _speed_Txt = "You've Activated the Speed Boost Power Up!\nEnjoy double the speed but becareful not to lose control!";
	void Start () {
		gameObject.SetActive (true);
	}

	void OnGUI(){
		GUI.Label(new Rect(Screen.width/2.0f - 150,Screen.height/1.25f, 800, 800), _gui_Txt);
	}

	// When player picks up a power up
	void OnCollisionEnter(Collision collision) {
		
		if (collision.collider.tag == "TankTag") {
			_collidedObject = collision.gameObject;
			ActivatePowerUp ();
		}
	}
		
	void ActivatePowerUp () {
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;
		int powerUpDuration = -1;

		if (P_Type == 0) {	 			// Speed Boost
			print ("Speed boost activated!");
			_collidedObject.GetComponent<CarProperties> ().Speed *= 2.0f;
			powerUpDuration = 5;

			_gui_Txt = _speed_Txt;
		} else if (P_Type == 1) {		// Ink Splatter
			print ("Ink Splatter Activated!");

			_splatterObject = new GameObject ("Splatter");

			_splatterObject.transform.parent = PlayerCanvas.transform;

			RawImage splatterImage = _splatterObject.AddComponent<RawImage> ();
			splatterImage.GetComponent<RectTransform> ().localPosition = PlayerCanvas.gameObject.GetComponent<RectTransform> ().rect.center;
			splatterImage.GetComponent<RectTransform> ().localScale = 3.0f * Vector3.one;
			splatterImage.texture = SplatterTex;

			powerUpDuration = 8;

			// The following lines fade out the splatter effect over time
			splatterImage.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
			splatterImage.CrossFadeAlpha(0.0f,8.0f,false);

			_gui_Txt = _splatter_Txt;

		} else if (P_Type == 2) {		// Place Holder
			print ("Some other powerup Activated");
		}

		if (powerUpDuration >= 0) {
			Invoke ("DeactivatePowerUp", powerUpDuration);
		}
	}

	void DeactivatePowerUp () {
		print ("PowerUp Deactivated");

		if (P_Type == 0) {
			_collidedObject.GetComponent<CarProperties> ().Speed *= 0.5f;

		} else if (P_Type == 1) {
			Destroy (_splatterObject);
		}

		Destroy (gameObject);
	}

	public void SetPowerUpType (int type) {
		P_Type = type;
	}
}
	