using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmoothCarMovement))]
public class SmoothCarMovementEditor : Editor
{
    SerializedProperty comProperty;

    void OnEnable()
    {
        comProperty = serializedObject.FindProperty("_centerOfMassVerticalOffset");
    }

    void OnSceneGUI()
    {
        SmoothCarMovement car = target as SmoothCarMovement;
        if (!car) return;
        serializedObject.Update();

        using (new Handles.DrawingScope(Color.red, car.transform.localToWorldMatrix))
        {

            Vector3 handlePos = Vector3.up * comProperty.floatValue;
            float handleSize = 0.25f;
            Handles.Label(handlePos + Vector3.up * handleSize * 2f, "CoM");


            if (Mathf.Abs(comProperty.floatValue) >= 0.1f)
            {
                Handles.DrawLine(Vector2.zero, handlePos);
            }
            else
            {
                Handles.color = Color.green;
            }
            
            Handles.SphereHandleCap(-1, Vector3.zero, Quaternion.identity, 0.15f, EventType.Repaint);

            EditorGUI.BeginChangeCheck();
            handlePos = Handles.FreeMoveHandle(handlePos, handleSize, Vector3.one, Handles.CircleHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Center Of Mass");
                comProperty.floatValue = Mathf.Round(handlePos.y * 4f) / 4f;
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }

        }
    }
}
