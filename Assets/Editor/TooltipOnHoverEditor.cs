using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(TooltipOnHover))]
public class TooltipOnHoverEditor : Editor {

    TooltipOnHover tooltipOnHover;
    string tooltipPlaintext;

    public override void OnInspectorGUI() {
        tooltipOnHover = (TooltipOnHover) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Tooltip Management Controls");
        EditorGUILayout.Space(5);

        tooltipPlaintext = EditorGUILayout.TextField(
            "Tooltip plaintext",
            tooltipPlaintext);

        if (GUILayout.Button("Set Tooltip")) {
            Tooltip newTooltip = new Tooltip(tooltipPlaintext);
            tooltipOnHover.tooltip = newTooltip;
        }
    }

}
