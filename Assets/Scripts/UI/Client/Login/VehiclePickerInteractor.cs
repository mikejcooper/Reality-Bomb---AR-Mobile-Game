using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehiclePickerInteractor : MonoBehaviour {

	public float MinXRelativeMovement = 0.5f;
	public VehiclePicker CarPickerObj;
	public Camera CameraObj;

	private Vector2 _lastTouchPos;
	private bool _hasPerformedGestureForTouch;

	void Update() {

		bool aTouch = false;
		bool isFirstFrame = false;
		Vector2 touchPos;

		if (Application.platform != RuntimePlatform.IPhonePlayer)
		{
			// use the input stuff
			aTouch = Input.GetMouseButton(0);
			touchPos = Input.mousePosition;
			isFirstFrame = Input.GetMouseButtonDown (0);
		} else {
			// use the iPhone Stuff
			aTouch = (Input.touchCount > 0);
			touchPos = Input.touches[0].position;
			isFirstFrame = Input.GetTouch (0).phase == TouchPhase.Began;
		}

		if (aTouch)
		{
			
			if (isFirstFrame) {
				_lastTouchPos = touchPos;
				_hasPerformedGestureForTouch = false;
			}



			if (!_hasPerformedGestureForTouch) {

				bool lastPosInRect = RectTransformUtility.RectangleContainsScreenPoint (GetComponent<RectTransform> (), _lastTouchPos, CameraObj);

				// Get movement of the finger since last frame
				Vector2 touchDeltaPosition = touchPos - _lastTouchPos;

				var minXMovement = GetComponent<RectTransform>().rect.width * MinXRelativeMovement;

				if (lastPosInRect && Mathf.Abs(touchDeltaPosition.x) >= minXMovement) {
					Debug.Log (touchDeltaPosition);
					_hasPerformedGestureForTouch = true;
					OnSwipe (Mathf.Sign (touchDeltaPosition.x));
				}
					
			}
		}
		_lastTouchPos = touchPos;
	}

	private void OnSwipe(float signum) {
		if (signum > 0) {
			Debug.Log ("swipe right");
			CarPickerObj.MoveLeft ();
		} else {
			Debug.Log ("swipe left");
			CarPickerObj.MoveRight ();
		}
	}

}
