using UnityEngine;
using UnityEditor;

public class RenameChildrenTool
{
    [MenuItem("Tools/Rename Selected Children")]
    static void RenameChildren()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Select a parent object first.");
            return;
        }

        Transform parent = Selection.activeTransform;

        int index = 1;
        foreach (Transform child in parent)
        {
            Undo.RecordObject(child.gameObject, "Rename Children");
            child.name = child.name + " " + index;
            index++;
        }

        Debug.Log("Children renamed successfully.");
    }
}
