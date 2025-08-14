using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideInInspectorWithCondition))]
public class HideInInspectorWithConditionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideInInspectorWithCondition att = (HideInInspectorWithCondition)attribute;
        bool reverse = att.reverse;
        string[] path = property.propertyPath.Split('.');
        path[path.Length - 1] = att.boolProperty;
        SerializedProperty boolProperty = property.serializedObject.FindProperty(string.Join(".", path));
        if (boolProperty != null && (boolProperty.boolValue != reverse))
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        return 0;
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        HideInInspectorWithCondition att = (HideInInspectorWithCondition)attribute;
        bool reverse = att.reverse;
        string[] path = property.propertyPath.Split('.');
        path[path.Length - 1] = att.boolProperty;
        SerializedProperty boolProperty = property.serializedObject.FindProperty(string.Join(".", path));
        if (boolProperty != null && (boolProperty.boolValue != reverse))
        {
            EditorGUI.PropertyField(position, property);
        }
    }
}