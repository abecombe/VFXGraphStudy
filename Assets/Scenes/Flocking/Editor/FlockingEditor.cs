using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Flocking))]
public class FlockingEditor : Editor
{
	SerializedProperty _flockingCS;

	SerializedProperty _numInstance;

	SerializedProperty _speedRange;
	SerializedProperty _forceWeight;
	SerializedProperty _perceptionRadius;
	SerializedProperty _maxSteerForce;

	SerializedProperty _targetObject;
	SerializedProperty _targetSeekForce;
	SerializedProperty _targetSeekClampDistance;

	SerializedProperty _scaleRange;
	SerializedProperty _animationSpeed;

	static GUIContent _textCenter = new GUIContent("Center");
	static GUIContent _textSize = new GUIContent("Size");
	static GUIContent _textAmplitude = new GUIContent("Amplitude");
	static GUIContent _textFrequency = new GUIContent("Frequency");

	void OnEnable()
	{
		_flockingCS = serializedObject.FindProperty("_flockingCS");

		_numInstance = serializedObject.FindProperty("_numInstance");

		_speedRange = serializedObject.FindProperty("_speedRange");
		_forceWeight = serializedObject.FindProperty("_forceWeight");
		_perceptionRadius = serializedObject.FindProperty("_perceptionRadius");
		_maxSteerForce = serializedObject.FindProperty("_maxSteerForce");

		_targetObject = serializedObject.FindProperty("_targetObject");
		_targetSeekForce = serializedObject.FindProperty("_targetSeekForce");
		_targetSeekClampDistance = serializedObject.FindProperty("_targetSeekClampDistance");

		_scaleRange = serializedObject.FindProperty("_scaleRange");
		_animationSpeed = serializedObject.FindProperty("_animationSpeed");
	}

	public override void OnInspectorGUI()
	{
		var flocking = target as Flocking;

		serializedObject.Update();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(_numInstance);
		if (EditorGUI.EndChangeCheck())
			flocking.NotifyConfigChange();
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Flocking", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(_speedRange);
		EditorGUILayout.PropertyField(_forceWeight);
		EditorGUILayout.PropertyField(_perceptionRadius);
		EditorGUILayout.PropertyField(_maxSteerForce);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("TargetSeeking", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(_targetObject);
		EditorGUILayout.PropertyField(_targetSeekForce);
		EditorGUILayout.PropertyField(_targetSeekClampDistance);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(_scaleRange);
		EditorGUILayout.PropertyField(_animationSpeed);

		serializedObject.ApplyModifiedProperties();
	}
}