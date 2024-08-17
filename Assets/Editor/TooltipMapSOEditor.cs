using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;
using UnityEngine.UI;

[CustomEditor(typeof(TooltipMapSO))]
public class ToolipMapSOEditor : Editor {

    TooltipMapSO tooltipMapSO;
    string tooltipTitle;
    string tooltipDescription;
    int tooltipRelevantBehaviorIndex;
    Image tooltipImage;
    TooltipKeyword tooltipKeyword;
    public override void OnInspectorGUI() {
        tooltipMapSO = (TooltipMapSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Controls");
        EditorGUILayout.Space(5);

        tooltipTitle = EditorGUILayout.TextField(
        "Tooltip Title",
        tooltipTitle);

        tooltipDescription = EditorGUILayout.TextField(
        "Tooltip Description",
        tooltipDescription);

        tooltipRelevantBehaviorIndex = EditorGUILayout.IntField(
        "Tooltip Description",
        tooltipRelevantBehaviorIndex);

        tooltipKeyword = (TooltipKeyword) EditorGUILayout.EnumPopup(
            "Tooltip Keyword",
            tooltipKeyword);

        tooltipImage = (Image) EditorGUILayout.ObjectField(
            "Tooltip Image",
            tooltipImage,
            typeof(Image),
            true);

        if (GUILayout.Button("Create Tooltip")) {
            tooltipMapSO.effectTooltipMappings.Add(new KeywordTooltipMapping(tooltipTitle, tooltipDescription, tooltipRelevantBehaviorIndex, tooltipImage, tooltipKeyword));
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
