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
    Sprite tooltipImage;
    TooltipKeyword tooltipKeyword;
    DescriptionToken.DescriptionIconType descriptionIconType;
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
        "Relevant Behavior Index",
        tooltipRelevantBehaviorIndex);

        tooltipKeyword = (TooltipKeyword) EditorGUILayout.EnumPopup(
            "Tooltip Keyword",
            tooltipKeyword);

        descriptionIconType = (DescriptionToken.DescriptionIconType) EditorGUILayout.EnumPopup(
            "Description Icon Type",
            descriptionIconType);

        tooltipImage = (Sprite) EditorGUILayout.ObjectField(
            "Tooltip Image",
            tooltipImage,
            typeof(Sprite),
            true);

        if (GUILayout.Button("Create Keyword Tooltip")) {
            tooltipMapSO.effectTooltipMappings.Add(new KeywordTooltipMapping(tooltipTitle, tooltipDescription, tooltipRelevantBehaviorIndex, tooltipImage, tooltipKeyword));
            save(tooltipMapSO);
        }

        if (GUILayout.Button("Create Description Icon Tooltip")) {
            tooltipMapSO.descriptionIconTooltipMappings.Add(new DescriptionIconTooltipMapping(descriptionIconType, tooltipTitle, tooltipDescription));
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
