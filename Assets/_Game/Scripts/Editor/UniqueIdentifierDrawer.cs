using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        string assetPath = AssetDatabase.GetAssetPath(_property.serializedObject.targetObject.GetInstanceID());
        string uniqueId = AssetDatabase.AssetPathToGUID(assetPath);
        _property.stringValue = uniqueId;

        Rect textFieldPosition = _position;
        textFieldPosition.height = 16;

        EditorGUI.LabelField(textFieldPosition, _label, new GUIContent(_property.stringValue));
    }
}
