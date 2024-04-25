using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(LabeledArrayAttribute))]
public class LabeledArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // try
        // {
        var path = property.propertyPath;
        int pos = int.Parse(path.Split('[').LastOrDefault().TrimEnd(']'));
        LabeledArrayAttribute attr = (LabeledArrayAttribute)attribute;
        EditorGUI.PropertyField(rect, property, new GUIContent(attr.names[pos / attr.repeat]), true);
        // catch
        // {
        //     EditorGUI.PropertyField(rect, property, label, true);
        // }
    }
}
