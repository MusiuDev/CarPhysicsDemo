using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SceneBezierSpline))]
public class BezierSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        SceneBezierSpline sceneSpline = target as SceneBezierSpline;
        BezierSpline spline = (sceneSpline).spline;

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
        SceneBezierSpline sceneSpline = target as SceneBezierSpline;
        BezierSpline spline = (sceneSpline).spline;
        var scope = new Handles.DrawingScope();
        using (scope)
        {
            var knots = spline.Knots;
            int index = 0;
            foreach (var knot in knots)
            {
                if (knot.rotation.x + knot.rotation.y + knot.rotation.z + knot.rotation.w == 0)
                {
                    knot.rotation = Quaternion.identity;
                }

                if (Tools.current == Tool.Scale)
                {
                    ScaleKnotHandles(knot, forwards: index < knots.Length - 1, index > 0);
                }

                if (Tools.current == Tool.Move)
                {
                    MoveKnot(knot);
                }

                if (Tools.current == Tool.Rotate)
                {
                    RotateKnot(knot);
                }
                index++;
            }

            if (Event.current.type == EventType.Repaint)
            {
                DrawSpline(spline);
            }
        }
    }

    private void ScaleKnotHandles(BezierKnot knot, bool forwards, bool backwards)
    {
        if (forwards)
        {
            EditorGUI.BeginChangeCheck();
            float newForwards = ScaleKnotHandle(knot.position, knot.Forward, knot.forwardsHandleSize);
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

                EditorUtility.SetDirty(target);
            }
        }

        if (backwards)
        {
            EditorGUI.BeginChangeCheck();
            float newBackwards = ScaleKnotHandle(knot.position, -knot.Forward, knot.backwardsHandleSize);
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

                EditorUtility.SetDirty(target);
            }
        }
    }

    private void MoveKnot(BezierKnot knot)
    {
        Handles.color = Color.red;
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.Slider(knot.position, knot.RawRight, 2f, Handles.ArrowHandleCap, 0.5f);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 delta = newPosition - knot.position;
            Vector3 axisDelta = Vector3.Project(delta, knot.RawRight);

            Undo.RecordObject(target, "Change Knot Offset");
            knot.position = knot.position + axisDelta;
            EditorUtility.SetDirty(target);
        }
    }

    private void RotateKnot(BezierKnot knot)
    {
        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();
        Quaternion newRotation = Handles.Disc(knot.RotationWithOffset, knot.position, knot.RawUp, 1.5f, false, 15f);
        if (EditorGUI.EndChangeCheck())
        {
            Quaternion delta = newRotation * Quaternion.Inverse(knot.rotation);
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
            EditorUtility.SetDirty(target);
        }
    }

    private float ScaleKnotHandle(Vector3 position, Vector3 axis, float knotHandleSize)
    {
        float handlesScaling = 0.5f;

        Vector3 handlePos = position + axis * knotHandleSize * handlesScaling;

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

    private void DrawSpline(BezierSpline spline)
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
        SceneBezierSpline sceneSpline = target as SceneBezierSpline;

        BezierSpline spline = (sceneSpline).spline;
        List<Checkpoint> checkpointCache = new List<Checkpoint>(group.Checkpoints);

        BezierKnot[] newKnots = new BezierKnot[checkpointCache.Count];

        for (int i = 0; i < checkpointCache.Count; i++)
        {
            Vector3 position = checkpointCache[i].transform.position;
            Quaternion rotation = checkpointCache[i].transform.rotation;
            BezierKnot newKnot = new BezierKnot(position, rotation, 5f, 5f);
            newKnots[i] = newKnot;
        }
        Undo.RecordObject(target, "Auto Update Knot References");
        spline.Knots = newKnots;
        EditorUtility.SetDirty(target);
    }
}
