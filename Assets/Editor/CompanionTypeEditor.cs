using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompanionTypeSO))]
public class CompanionTypeEditor : Editor {

    string companionAbilityClassName = "RetainCards";
    public override void OnInspectorGUI() {
        CompanionTypeSO companionTypeSO = (CompanionTypeSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Procedure Controls");
        EditorGUILayout.Space(5);

        companionAbilityClassName = EditorGUILayout.TextField(
            "New ability classname",
            companionAbilityClassName);

        if (GUILayout.Button("Add Companion Ability")) {
            CompanionAbility newAbility = InstantiateFromClassname.Instantiate<CompanionAbility>(
                companionAbilityClassName, 
                new object[] {});

            if(newAbility == null) {
                Debug.LogError("Failed to instantiate companion ability , " +
                "please check Scripts/Companions/CompanionAbilities/* to verify the className");
            }
            else {
                companionTypeSO.abilities.Add(newAbility);
            }
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(companionTypeSO);
            AssetDatabase.SaveAssets();
        }
    }

}
