using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CheckpointGroup))]
public class CheckpointGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Auto Assign Checkpoints"))
        {
            AutoAssignCheckpoints();
        }
    }


    private void AutoAssignCheckpoints()
    {
        CheckpointGroup group = target as CheckpointGroup;
        if (!group) return;

        Checkpoint[] childCheckpoints = group.gameObject.GetComponentsInChildren<Checkpoint>();
        if (childCheckpoints == null || childCheckpoints.Length == 0) return;

        serializedObject.Update();

        SerializedProperty checkpointsProp = serializedObject.FindProperty("_checkpoints");
        checkpointsProp.arraySize = childCheckpoints.Length;

        for (int i = 0; i < childCheckpoints.Length; i++)
        {
            SerializedProperty chk = checkpointsProp.GetArrayElementAtIndex(i);
            chk.objectReferenceValue = childCheckpoints[i];
        }
        serializedObject.ApplyModifiedProperties();
    }
}
