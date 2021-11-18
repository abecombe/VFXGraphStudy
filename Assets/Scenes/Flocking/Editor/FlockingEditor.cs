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

		EditorGUILayout.Space(1);

		EditorGUILayout.LabelField("Instancing", EditorStyles.boldLabel);
		EditorGUILayout.Space(1);
		using (new EditorGUI.IndentLevelScope(1))
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_numInstance);
			if (EditorGUI.EndChangeCheck())
				flocking.NotifyConfigChange();
		}

		EditorGUILayout.Space(5);

		EditorGUILayout.LabelField("Flocking", EditorStyles.boldLabel);
		EditorGUILayout.Space(1);
		using (new EditorGUI.IndentLevelScope(1))
		{
			var speedRange = flocking.speedRange;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Speed Range");
			GUILayout.Label("Min", GUILayout.Width(28));
			speedRange.x = EditorGUILayout.FloatField(speedRange.x);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("Max", GUILayout.Width(28));
			speedRange.y = EditorGUILayout.FloatField(speedRange.y);
			EditorGUILayout.EndHorizontal();
			flocking.speedRange = speedRange;

			EditorGUILayout.Space(3);

			var forceWeight = flocking.forceWeight;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Force Weight");
			GUILayout.Label("alignment", GUILayout.Width(66));
			forceWeight.x = EditorGUILayout.FloatField(forceWeight.x);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("cohesion", GUILayout.Width(66));
			forceWeight.y = EditorGUILayout.FloatField(forceWeight.y);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("Separation", GUILayout.Width(66));
			forceWeight.z = EditorGUILayout.FloatField(forceWeight.z);
			EditorGUILayout.EndHorizontal();
			flocking.forceWeight = forceWeight;

			EditorGUILayout.Space(3);

			var perceptionRadius = flocking.perceptionRadius;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Perception Radius");
			GUILayout.Label("alignment", GUILayout.Width(66));
			perceptionRadius.x = EditorGUILayout.FloatField(perceptionRadius.x);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("cohesion", GUILayout.Width(66));
			perceptionRadius.y = EditorGUILayout.FloatField(perceptionRadius.y);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("Separation", GUILayout.Width(66));
			perceptionRadius.z = EditorGUILayout.FloatField(perceptionRadius.z);
			EditorGUILayout.EndHorizontal();
			flocking.perceptionRadius = perceptionRadius;

			EditorGUILayout.Space(3);

			EditorGUILayout.PropertyField(_maxSteerForce);
		}

		EditorGUILayout.Space(5);

		EditorGUILayout.LabelField("Target Seeking", EditorStyles.boldLabel);
		EditorGUILayout.Space(1);
		using (new EditorGUI.IndentLevelScope(1))
		{
			EditorGUILayout.PropertyField(_targetObject);
			flocking.targetSeekForce = EditorGUILayout.FloatField("Seek Force", flocking.targetSeekForce);
			flocking.targetSeekClampDistance = EditorGUILayout.FloatField("Seek Clamp Distance", flocking.targetSeekClampDistance);
		}

		EditorGUILayout.Space(5);

		EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
		EditorGUILayout.Space(1);
		using (new EditorGUI.IndentLevelScope(1))
		{
			var scaleRange = flocking.scaleRange;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Scale Range");
			GUILayout.Label("Min", GUILayout.Width(28));
			scaleRange.x = EditorGUILayout.FloatField(scaleRange.x);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			GUILayout.Label("Max", GUILayout.Width(28));
			scaleRange.y = EditorGUILayout.FloatField(scaleRange.y);
			EditorGUILayout.EndHorizontal();
			flocking.scaleRange = scaleRange;

			EditorGUILayout.Space(3);

			EditorGUILayout.PropertyField(_animationSpeed);
		}

		serializedObject.ApplyModifiedProperties();
	}
}