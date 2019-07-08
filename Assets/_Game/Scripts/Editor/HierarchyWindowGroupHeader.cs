using UnityEngine;
using UnityEditor;

// Hierarchy Window Group Header
// http://diegogiacomelli.com.br/unitytips-hierarchy-window-group-header

[InitializeOnLoad]
public static class HierarchyWindowGroupHeader
{
    static HierarchyWindowGroupHeader()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject != null && gameObject.name.StartsWith("---", System.StringComparison.Ordinal)) {
            EditorGUI.DrawRect(selectionRect, Color.gray);

            selectionRect.height -= 2;
            EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("-", "").ToUpperInvariant());
        }
    }
}