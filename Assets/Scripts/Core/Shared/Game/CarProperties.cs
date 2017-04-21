using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CarProperties : MonoBehaviour
{
	[SerializeField] private float _startingSpeedLim;
	[SerializeField] private float _maxSpeedLim;
	[SerializeField] private float _minSpeedLim;
	[HideInInspector]
	public float SpeedLimit;
	public float SafeSpeedLimit { get { return Math.Max(Math.Min(SpeedLimit, _maxSpeedLim), _minSpeedLim); }}
	[SerializeField] private float _startingScale;
	[SerializeField] private float _maxScale;
	[SerializeField] private float _minScale;
	[HideInInspector]
	public float Scale;
	public float SafeScale { get { return Math.Max(Math.Min(Scale, _maxScale), _minScale); }}
	[SerializeField] private float _startingAccel;
	[SerializeField] private float _maxAccel;
	[SerializeField] private float _minAccel;
	[HideInInspector]
	public float Accel;
	public float SafeAccel { get { return Math.Max(Math.Min(Accel, _maxAccel), _minAccel); }}

	[SerializeField] private float _startingTurnRate;
	[SerializeField] private float _maxTurnRate;
	[SerializeField] private float _minTurnRate;
	[HideInInspector]
	public float TurnRate;
	public float SafeTurnRate { get { return Math.Max(Math.Min(TurnRate, _maxTurnRate), _minTurnRate); }}

	[HideInInspector]
	public float OriginalHue;

	#if UNITY_EDITOR
	public void ShowEditGUI() {
		_startingSpeedLim = EditorGUILayout.FloatField("Starting Speed Limit", _startingSpeedLim);
		_maxSpeedLim = EditorGUILayout.FloatField("Max Speed Limit", _maxSpeedLim);
		_minSpeedLim = EditorGUILayout.FloatField("Min Speed Limit", _minSpeedLim);
		_startingScale = EditorGUILayout.FloatField("Starting Scale", _startingScale);
		_maxScale = EditorGUILayout.FloatField("Max Scale", _maxScale);
		_minScale = EditorGUILayout.FloatField("Min Scale", _minScale);
		_startingAccel = EditorGUILayout.FloatField("Starting Acceleration", _startingAccel);
		_maxAccel = EditorGUILayout.FloatField("Max Acceleration", _maxAccel);
		_minAccel = EditorGUILayout.FloatField("Min Acceleration", _minAccel);
		_startingTurnRate = EditorGUILayout.FloatField("Starting Turn Rate", _startingTurnRate);
	}
	#endif

	void Awake () {
		SpeedLimit = _startingSpeedLim;
		Scale = _startingScale;
		Accel = _startingAccel;
		TurnRate = _startingTurnRate;
	}


}


