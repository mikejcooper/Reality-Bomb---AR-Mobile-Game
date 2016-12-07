using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{

	private bool button_entry;
	private bool button_exit;
	private bool button_held;
	public bool fire_this_click;

	void Start ()
	{
		button_entry = false;
		button_exit = false;
		fire_this_click = false;
	}

	public void Update()
	{

		// OnPointerDown
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{

			// Check if finger is over a UI element
			if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
			{
				Debug.Log("BEGAN");

				button_entry = true;
				button_exit = false;
			}
		}
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
		{

			// Check if finger is over a UI element
			if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
			{
				Debug.Log("ENDED");


				button_entry = false;
				button_exit = true;
				fire_this_click = false;
			}
		}




	}

		

	public virtual void OnPointerDown(PointerEventData ped)
	{
		button_entry = true;
		button_exit = false;
	}

	public virtual void OnPointerUp(PointerEventData ped)
	{
		button_entry = false;
		button_exit = true;
		fire_this_click = false;
	}

	// fire button is clicked
	public bool GetButtonDown()
	{
		return button_entry;
	}	

	// fire button is released
	public bool GetButtonUp()
	{
		return button_exit;
	}

	public bool GetFireThisClick()
	{
		return fire_this_click;
	}

	public void SetFireThisClick()
	{
		fire_this_click = true;
	}
		
}

