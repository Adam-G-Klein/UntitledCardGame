using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(TooltipMapSO))]
public class ToolipMapSOEditor : Editor {

    TooltipMapSO tooltipMapSO;
    string tooltipPlaintext;
    TooltipKeyword tooltipKeyword;
    public override void OnInspectorGUI() {
        tooltipMapSO = (TooltipMapSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Map Controls");
        EditorGUILayout.Space(5);

        tooltipPlaintext = EditorGUILayout.TextField(
        "Tooltip plaintext",
        tooltipPlaintext);

        tooltipKeyword = (TooltipKeyword) EditorGUILayout.EnumPopup(
            "Tooltip Keyword",
            tooltipKeyword);

        if (GUILayout.Button("Create Tooltip")) {
            Tooltip newTooltip = new Tooltip(tooltipPlaintext);
            tooltipMapSO.effectTooltipMappings.Add(new KeywordTooltipMapping(newTooltip, tooltipKeyword));
            save(tooltipMapSO);
        }
    }
    private void save(TooltipMapSO map) {
        // These three calls cause the asset to actually be modified
        // on disc when we hit the button
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();

    }

}
