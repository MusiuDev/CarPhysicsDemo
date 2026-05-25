using UnityEngine;
using UnityEditor;

public static class EditorUtils
{
    [MenuItem("Custom Tools/Randomize Vertical Rotation")]
    public static void RandomizeVerticalRotation()
    {
        var transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        foreach (var item in transforms)
        {
            Undo.RecordObject(item, "Random Vertical Rotation");
            item.Rotate(0, Random.Range(0, 360f), 0);
            EditorUtility.SetDirty(item);
        }
    }
}
