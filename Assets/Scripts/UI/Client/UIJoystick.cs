using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {

	private Image _backgroundImg;
	private Image _joystickImg;
	private Vector3 _inputVector;
	private bool _isPointerDown;

	void Start () {
		_backgroundImg = GetComponent<Image>();
		_joystickImg = transform.GetChild (0).GetComponent<Image>();
	}

	public virtual void OnDrag(PointerEventData ped)
	{
		Vector2 pos; 

		if(RectTransformUtility.ScreenPointToLocalPointInRectangle (_backgroundImg.rectTransform, ped.position, ped.pressEventCamera, out pos))
		{
			pos.x = (pos.x / _backgroundImg.rectTransform.sizeDelta.x);
			pos.y = (pos.y / _backgroundImg.rectTransform.sizeDelta.y);

			// Convert to 3d vector
			_inputVector = new Vector3 (pos.x * 2, 0, pos.y * 2);

			// Normalise position
			_inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized: _inputVector;

			// Move Joystick img
			_joystickImg.rectTransform.anchoredPosition = new Vector3 (_inputVector.x * _backgroundImg.rectTransform.sizeDelta.x / 2, _inputVector.z * _backgroundImg.rectTransform.sizeDelta.y / 2);

		}
	}

	public virtual void OnPointerDown(PointerEventData ped)
	{
		_isPointerDown = true;
		OnDrag (ped);
	}

	// Set joystick to centre of background image on release
	public virtual void OnPointerUp(PointerEventData ped)
	{
		_isPointerDown = false;
		_inputVector = Vector3.zero;
		_joystickImg.rectTransform.anchoredPosition = Vector3.zero;
	}

	// Change if we want tanks to come to a gradual stop
	public float Horizontal()
	{
		if (_inputVector.x != 0)
			return _inputVector.x;
		else
			return _inputVector.x;
	}

	public float Vertical()
	{
		if (_inputVector.x != 0)
			return _inputVector.z;
		else
			return _inputVector.z;
	}

	public bool IsDragging()
	{
		return _isPointerDown;
	}
}

