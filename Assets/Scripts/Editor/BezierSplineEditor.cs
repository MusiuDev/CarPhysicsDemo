using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor
{
    private SerializedProperty _knotsProperty;

    void OnEnable()
    {
        _knotsProperty = serializedObject.FindProperty("_knots");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        BezierSpline spline = target as BezierSpline;

        CheckpointGroup checkpointGroup = spline?.GetComponent<CheckpointGroup>();

        EditorGUI.BeginDisabledGroup(checkpointGroup == null);

        if (GUILayout.Button("Get Knots From Checkpoint Group"))
        {
            GetKnotsFromCheckpointGroup(checkpointGroup);
        }
        EditorGUI.EndDisabledGroup();
    }

    void OnSceneGUI()
    {
        BezierSpline spline = target as BezierSpline;
        DrawSpline(spline);

        foreach (var item in spline.Knots)
        {

        }
    }

    private static void DrawSpline(BezierSpline spline)
    {
        if (Event.current.type == EventType.Repaint)
        {
            using (new Handles.DrawingScope())
            {
                Handles.color = Color.orange;
                List<Vector3> path = spline.GetFullPath(0.5f);
                if (path.Count > 1)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        Vector3 point = path[i];
                        Handles.SphereHandleCap(-1, point, Quaternion.identity, 0.2f, EventType.Repaint);
                    }
                }
                Handles.DrawPolyLine(path.ToArray());
            }
        }
    }

    private void GetKnotsFromCheckpointGroup(CheckpointGroup group)
    {
        BezierSpline curve = target as BezierSpline;
        List<TransformBezierKnot> knotsCache = new List<TransformBezierKnot>(curve.Knots);

        List<Checkpoint> checkpointCache = new List<Checkpoint>(group.Checkpoints);

        _knotsProperty.arraySize = checkpointCache.Count;

        Undo.RecordObject(target, "Auto Update Knot References");
        for (int i = 0; i < checkpointCache.Count; i++)
        {
            float handleSize = 1f;
            if (knotsCache.Count > i)
            {
                if (knotsCache.Count > i)
                {
                    handleSize = knotsCache[i].handleSize;
                }
            }
            SerializedProperty knot = _knotsProperty.GetArrayElementAtIndex(i);
            knot.FindPropertyRelative(nameof(TransformBezierKnot.transform)).objectReferenceValue = checkpointCache[i].transform;
            knot.FindPropertyRelative(nameof(TransformBezierKnot.handleSize)).floatValue = handleSize;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
