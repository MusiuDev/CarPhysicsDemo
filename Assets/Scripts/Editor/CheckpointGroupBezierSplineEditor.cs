using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CheckpointGroupBezierSpline))]
public class CheckpointGroupBezierSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        CheckpointGroupBezierSpline sceneSpline = target as CheckpointGroupBezierSpline;

        CheckpointGroup checkpointGroup = sceneSpline?.GetComponent<CheckpointGroup>();

        EditorGUI.BeginDisabledGroup(checkpointGroup == null);

        if (GUILayout.Button("Get Knots From Checkpoint Group"))
        {
            GetKnotsFromCheckpointGroup(checkpointGroup);
        }
        EditorGUI.EndDisabledGroup();
    }

    void OnSceneGUI()
    {
        CheckpointGroupBezierSpline sceneSpline = target as CheckpointGroupBezierSpline;
        if (sceneSpline.knots == null || sceneSpline.knots.Length == 0) return;
        var scope = new Handles.DrawingScope();
        using (scope)
        {
            var knots = sceneSpline.knots;
            int index = 0;
            bool allToolsActive = Tools.current == Tool.Rect;
            foreach (var knot in knots)
            {
                if (!knot) continue;
                
                if (allToolsActive || Tools.current == Tool.Scale)
                {
                    ScaleKnotHandles(knot, forwards: index < knots.Length - 1, index > 0);
                }

                if (allToolsActive || Tools.current == Tool.Move)
                {
                    MoveKnot(knot);
                }

                if (allToolsActive || Tools.current == Tool.Rotate)
                {
                    RotateKnot(knot);
                }
                index++;
            }

            if (Event.current.type == EventType.Repaint)
            {
                DrawSplinePath(BezierSpline.GetFullPath(knots, sceneSpline.segmentDistance));
            }
        }
    }

    private void ScaleKnotHandles(TransformBezierKnot knot, bool forwards, bool backwards)
    {
        if (forwards)
        {
            EditorGUI.BeginChangeCheck();
            float newForwards = ScaleKnotHandle(knot.Position, knot.Forward, knot.forwardsHandleSize);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Scale Handle(s)");

                if (Event.current.alt && knot.forwardsHandleSize > 0.01f) //If the original value is too low, don't scale, to avoid edge case behaviors.
                {
                    knot.backwardsHandleSize *= newForwards / knot.forwardsHandleSize;
                }
                else if (Event.current.shift)
                {
                    knot.backwardsHandleSize = newForwards;
                }
                knot.forwardsHandleSize = newForwards;

                EditorUtility.SetDirty(knot);
            }
        }

        if (backwards)
        {
            EditorGUI.BeginChangeCheck();
            float newBackwards = ScaleKnotHandle(knot.Position, -knot.Forward, knot.backwardsHandleSize);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Scale Handle(s)");

                if (Event.current.alt && knot.backwardsHandleSize > 0.01f)
                {
                    knot.forwardsHandleSize *= newBackwards / knot.backwardsHandleSize;
                }
                else if (Event.current.shift)
                {
                    knot.forwardsHandleSize = newBackwards;
                }
                knot.backwardsHandleSize = newBackwards;

                EditorUtility.SetDirty(knot);
            }
        }
    }

    private void MoveKnot(TransformBezierKnot knot)
    {
        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.Slider(knot.Position, knot.RawRight, 0.5f, PositionOffsetHandleCap, 0.5f);
        Handles.SphereHandleCap(-1, knot.RawPosition, Quaternion.identity, 0.25f, EventType.Repaint);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Knot Offset");
            float newOffset = knot.InverseTransformPoint(newPosition).x;
            knot.positionOffset = newOffset;
            EditorUtility.SetDirty(knot);
        }
    }

    private void RotateKnot(TransformBezierKnot knot)
    {
        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();
        Quaternion newRotation = Handles.Disc(knot.Rotation, knot.Position, knot.RawUp, 1.5f, false, 15f);
        if (EditorGUI.EndChangeCheck())
        {
            Quaternion delta = newRotation * Quaternion.Inverse(knot.RawRotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);

            if (Mathf.Abs(angle) < 0.001f)
            {
                angle = 0f;
            }
            else
            {
                angle = angle * Mathf.Sign(Vector3.Dot(axis, knot.RawUp));
            }

            Undo.RecordObject(target, "Change Knot Rotation");
            knot.rotationOffset = angle;
            EditorUtility.SetDirty(knot);
        }
    }

    private float ScaleKnotHandle(Vector3 position, Vector3 axis, float knotHandleSize)
    {
        float handlesScaling = 0.5f;

        Vector3 handlePos = position + handlesScaling * knotHandleSize * axis;

        Handles.color = Event.current.shift ? Color.blueViolet : Color.blue;

        Handles.CapFunction capFunction = Handles.CircleHandleCap;
        float handleCapSize = 0.25f;
        float handleCapMult = 1f;
        if (Event.current.alt || Event.current.shift)
        {
            capFunction = Handles.SphereHandleCap;
            handleCapMult = 2f;
        }

        Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, handleCapSize * handleCapMult, Vector3.one * 0.1f, capFunction);

        Handles.color = Color.blue;
        Handles.DrawLine(position, handlePos - axis * handleCapSize);

        Vector3 axisHandlePos = Vector3.Project((newHandlePos - position), axis);
        float newHandleSize = axisHandlePos.magnitude / handlesScaling;

        return newHandleSize;
    }

    private void DrawSplinePath(SplinePath path)
    {
        Handles.color = Color.cyan;
        if (path.pathPoints.Count > 1)
        {
            for (int i = 0; i < path.pathPoints.Count; i++)
            {
                Vector3 point = path.pathPoints[i];
                Handles.SphereHandleCap(-1, point, Quaternion.identity, 0.2f, EventType.Repaint);
            }
        }
    }

    private void GetKnotsFromCheckpointGroup(CheckpointGroup group)
    {
        CheckpointGroupBezierSpline sceneSpline = target as CheckpointGroupBezierSpline;

        TransformBezierKnot[] childrenKnots = sceneSpline.GetComponentsInChildren<TransformBezierKnot>();
        sceneSpline.knots = childrenKnots;
        EditorUtility.SetDirty(target);
    }

    private static void PositionOffsetHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
    {
        Handles.CylinderHandleCap(controlID, position, rotation, size, eventType);
        Handles.ArrowHandleCap(controlID, position, rotation, size * 3f, eventType);
        Handles.ArrowHandleCap(controlID, position, Quaternion.Euler(0, 180f, 0) * rotation, size * 3f, eventType);
    }
}
