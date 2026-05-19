using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(SmoothCarCameraFollow))]
public class SmoothCarCameraEditor : Editor
{

    private bool _autoUpdate;
    private MethodInfo _cameraUpdateMethod;

    private SerializedProperty _followTargetProp;

    void OnEnable()
    {
        _cameraUpdateMethod = typeof(SmoothCarCameraFollow).GetMethod("FollowTarget", BindingFlags.NonPublic | BindingFlags.Instance);
        _followTargetProp = serializedObject.FindProperty("_carTransform");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Editor");
        EditorGUI.indentLevel++;

        _autoUpdate = EditorGUILayout.Toggle("Auto Update", _autoUpdate);

        if (GUILayout.Button("Update Pos") || _autoUpdate)
        {
            Debug.Log("Updating camera position...");
            _cameraUpdateMethod.Invoke(target, new object[] { (Transform)_followTargetProp.objectReferenceValue });
        }

        EditorGUI.indentLevel--;
    }


    void OnSceneGUI()
    {

    }
}
