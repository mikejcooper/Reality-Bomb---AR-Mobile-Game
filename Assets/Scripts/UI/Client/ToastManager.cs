using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour {

	public int ToastDuration = 10;
	public float AnimationSpeed = 0.05f;

	private RectTransform _rectTransform;
	private Text _text;
	private string _currentMessage;
	private float _currentOffset;
	private string _queuedMessage;
	private float _opacity = 1.0f;

	void Start () {
		_rectTransform = GetComponent<RectTransform> ();
		_currentOffset = _rectTransform.sizeDelta.y;
		_text = GetComponentInChildren<Text> ();
	}

	void FixedUpdate () {
		if (_currentMessage != null) {
			_currentOffset = Mathf.Max(0.0f, _currentOffset - AnimationSpeed);
		} else {
			_currentOffset = Mathf.Min(1.0f, _currentOffset + AnimationSpeed);
		}

		var from = new Vector2 (_rectTransform.anchoredPosition.x, 0);
		var to = new Vector2 (_rectTransform.anchoredPosition.x, _rectTransform.sizeDelta.y);
		_rectTransform.anchoredPosition = Vector3.Slerp (from, to, _currentOffset);

		if (_queuedMessage != null && _opacity > 0.0f) {
			_opacity = Mathf.Max (0.0f, _opacity - 0.1f);
		} else if (_queuedMessage != null && _opacity <= 0.0f) {
			_currentMessage = _queuedMessage;
			_text.text = _currentMessage;
			_queuedMessage = null;
		} else {
			_opacity = Mathf.Min (1.0f, _opacity + 0.1f);
		}
		var color = _text.color;
		color.a = _opacity;
		_text.color = color;
	}



	public void ShowMessage (string message) {
		DisplayMessage (message);
	}

	private void DisplayMessage (string message) {
		if (_currentMessage != null) {
			if (System.String.Equals (_currentMessage, message, System.StringComparison.Ordinal) || 
				System.String.Equals (_queuedMessage, message, System.StringComparison.Ordinal)) {
				Debug.Log ("Not displaying message as it's the same as the current");
				return;
			}
			_queuedMessage = message;
		} else {
			_currentMessage = message;
			_text.text = _currentMessage;
		}
			
		CancelInvoke ();
		Invoke ("HideMessage", ToastDuration);
	}

	private void HideMessage () {
		_currentMessage = null;
	}
}
