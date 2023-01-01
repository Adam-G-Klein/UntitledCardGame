using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Reference), true)]
public class ReferenceDrawer : PropertyDrawer
{
    private const float LINE_HEIGHT = 20f;
    private SerializedProperty useConstant;
    private SerializedProperty constantValue;
    private SerializedProperty variable;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float height = LINE_HEIGHT;
        useConstant = property.FindPropertyRelative("UseConstant");
        constantValue = property.FindPropertyRelative("ConstantValue");
        variable = property.FindPropertyRelative("Variable");

        if (property.isExpanded) {
            height += EditorGUI.GetPropertyHeight(useConstant);
            if (useConstant.boolValue) {
                height += EditorGUI.GetPropertyHeight(constantValue);
            } else {
                height += EditorGUI.GetPropertyHeight(variable);
        }
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        useConstant = property.FindPropertyRelative("UseConstant");
        constantValue = property.FindPropertyRelative("ConstantValue");
        variable = property.FindPropertyRelative("Variable");
        
        label = EditorGUI.BeginProperty(position, label, property);
        Rect foldoutRect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded) {
            EditorGUI.indentLevel++;
            position.y += LINE_HEIGHT;
            position.height = LINE_HEIGHT;
            EditorGUI.PropertyField(position, useConstant, new GUIContent("Use Constant"));

            position.y += LINE_HEIGHT;
            if (useConstant.boolValue) {
                position.height = EditorGUI.GetPropertyHeight(constantValue);
            } else {
                position.height = LINE_HEIGHT;
            }
            EditorGUI.PropertyField(position, useConstant.boolValue ? constantValue : variable, new GUIContent("Value"), true);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }
}