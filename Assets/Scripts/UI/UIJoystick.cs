using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {

	private Image m_BackgroundImg;
	private Image m_JoystickImg;
	private Vector3 m_InputVector;

	void Start () {
		m_BackgroundImg = GetComponent<Image>();
		m_JoystickImg = transform.GetChild (0).GetComponent<Image>();
	}

	public virtual void OnDrag(PointerEventData ped)
	{
		Vector2 pos; 

		if(RectTransformUtility.ScreenPointToLocalPointInRectangle (m_BackgroundImg.rectTransform, ped.position, ped.pressEventCamera, out pos))
		{
			pos.x = (pos.x / m_BackgroundImg.rectTransform.sizeDelta.x);
			pos.y = (pos.y / m_BackgroundImg.rectTransform.sizeDelta.y);

			// Convert to 3d vector
			m_InputVector = new Vector3 (pos.x * 2, 0, pos.y * 2);

			// Normalise position
			m_InputVector = (m_InputVector.magnitude > 1.0f) ? m_InputVector.normalized: m_InputVector;

			// Move Joystick img
			m_JoystickImg.rectTransform.anchoredPosition = new Vector3 (m_InputVector.x * m_BackgroundImg.rectTransform.sizeDelta.x / 2, m_InputVector.z * m_BackgroundImg.rectTransform.sizeDelta.y / 2);

		}
	}

	public virtual void OnPointerDown(PointerEventData ped)
	{
		OnDrag (ped);
	}

	// Set joystick to centre of background image on release
	public virtual void OnPointerUp(PointerEventData ped)
	{

		m_InputVector = Vector3.zero;
		m_JoystickImg.rectTransform.anchoredPosition = Vector3.zero;
	}

	// Change if we want tanks to come to a gradual stop
	public float Horizontal()
	{
		if (m_InputVector.x != 0)
			return m_InputVector.x;
		else
			return m_InputVector.x;
	}

	public float Vertical()
	{
		if (m_InputVector.x != 0)
			return m_InputVector.z;
		else
			return m_InputVector.z;
	}
}

