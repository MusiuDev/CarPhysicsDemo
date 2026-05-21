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
        var scope = new Handles.DrawingScope();
        using (scope)
        {
            var knots = spline.Knots;
            float handlesScaling = 0.5f;
            int index = 0;
            foreach (var knot in knots)
            {
                if (knot.rotation.x + knot.rotation.y + knot.rotation.z + knot.rotation.w == 0)
                {
                    knot.rotation = Quaternion.identity;
                }

                if (index < knots.Count - 1)
                {
                    EditorGUI.BeginChangeCheck();

                    Handles.color = Event.current.alt ? Color.azure : Color.blue;
                    Vector3 newForward = Handles.FreeMoveHandle(knot.ScaledForwardsHandle(handlesScaling), 0.25f, Vector3.one * 0.1f, Handles.CircleHandleCap);

                    Handles.color = Color.blue;
                    Handles.DrawLine(knot.position, knot.ScaledForwardsHandle(handlesScaling));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Change Handle Size");
                        Vector3 newHandle = Vector3.Project((newForward - knot.position), knot.Forward);
                        float newHandleSize = newHandle.magnitude / handlesScaling;

                        if (Event.current.alt)
                        {
                            float handleSizeRatio = newHandleSize / Mathf.Max(knot.forwardsHandleSize, 0.01f);
                            knot.backwardsHandleSize *= handleSizeRatio;
                        }
                        knot.forwardsHandleSize = newHandleSize;
                        EditorUtility.SetDirty(target);
                    }
                }

                if (index > 0)
                {
                    EditorGUI.BeginChangeCheck();
                    Handles.color = Event.current.alt ? Color.azure : Color.blue;
                    Vector3 newBackward = Handles.FreeMoveHandle(knot.ScaledBackwardsHandle(handlesScaling), 0.25f, Vector3.one * 0.1f, Handles.CircleHandleCap);
                    Handles.color = Color.blue;

                    Handles.DrawLine(knot.position, knot.ScaledBackwardsHandle(handlesScaling));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Change Handle Size");
                        Vector3 newHandle = Vector3.Project((newBackward - knot.position), knot.Forward);
                        float newHandleSize = newHandle.magnitude / handlesScaling;

                        if (Event.current.alt)
                        {
                            float handleSizeRatio = newHandleSize / Mathf.Max(knot.backwardsHandleSize, 0.01f);
                            knot.forwardsHandleSize *= handleSizeRatio;
                        }

                        knot.backwardsHandleSize = newHandleSize;
                        EditorUtility.SetDirty(target);
                    }
                }

                Handles.color = Color.red;
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.FreeMoveHandle(knot.position, 0.35f, Vector3.one * 0.25f, Handles.CircleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 delta = newPosition - knot.position;
                    Vector3 axisDelta = Vector3.Project(delta, knot.Right);

                    Undo.RecordObject(target, "Change Knot Offset");
                    knot.position = knot.position + axisDelta;
                    EditorUtility.SetDirty(target);
                }

                Handles.color = Color.green;
                EditorGUI.BeginChangeCheck();
                Quaternion newRotation = Handles.Disc(knot.rotation, knot.position, knot.Up, 1.5f, false, 15f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change Knot Rotation");
                    knot.rotation = newRotation;
                    EditorUtility.SetDirty(target);
                }

                index++;
            }

            if (Event.current.type == EventType.Repaint)
            {
                DrawSpline(spline);
            }
        }
    }

    private static void DrawSpline(BezierSpline spline)
    {
        Handles.color = Color.cyan;
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

    private void GetKnotsFromCheckpointGroup(CheckpointGroup group)
    {
        BezierSpline curve = target as BezierSpline;
        List<BezierKnot> knotsCache = new List<BezierKnot>(curve.Knots);

        List<Checkpoint> checkpointCache = new List<Checkpoint>(group.Checkpoints);
        _knotsProperty.arraySize = checkpointCache.Count;

        Undo.RecordObject(target, "Auto Update Knot References");
        for (int i = 0; i < checkpointCache.Count; i++)
        {
            Vector3 position = checkpointCache[i].transform.position;
            Quaternion rotation = checkpointCache[i].transform.rotation;
            float forwardsHandleSize = 5f;
            float backwardsHandleSize = 5f;
            if (knotsCache.Count > i)
            {
                forwardsHandleSize = knotsCache[i].forwardsHandleSize;
                backwardsHandleSize = knotsCache[i].backwardsHandleSize;
            }

            BezierKnot newKnot = new BezierKnot(position, rotation, forwardsHandleSize, backwardsHandleSize);
            SetSerializedKnotAtIndex(newKnot, i);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void SetSerializedKnotAtIndex(BezierKnot knot, int index)
    {
        if (_knotsProperty.arraySize <= index)
        {
            Debug.LogError("Trying to set a knot at an out of range index");
            return;
        }

        SerializedProperty serializedKnot = _knotsProperty.GetArrayElementAtIndex(index);
        serializedKnot.FindPropertyRelative(nameof(BezierKnot.position)).vector3Value = knot.position;
        serializedKnot.FindPropertyRelative(nameof(BezierKnot.rotation)).quaternionValue = knot.rotation;
        serializedKnot.FindPropertyRelative(nameof(BezierKnot.forwardsHandleSize)).floatValue = knot.forwardsHandleSize;
        serializedKnot.FindPropertyRelative(nameof(BezierKnot.backwardsHandleSize)).floatValue = knot.backwardsHandleSize;
    }
}
