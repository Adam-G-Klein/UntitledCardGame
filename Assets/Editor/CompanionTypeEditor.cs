using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompanionTypeSO))]
public class CompanionTypeEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    int abilityIndex = 0;

    public override void OnInspectorGUI() {
        CompanionTypeSO companionTypeSO = (CompanionTypeSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);
        abilityIndex = EditorGUILayout.IntField("Ability Index", abilityIndex);

        if (GUILayout.Button("Add Ability Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(),
                new object[] {});

            if(newEffect == null) {
                Debug.LogError("Failed to instantiate effect step, " +
                "please check Scripts/Effects/EffectSteps/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else {
                if (companionTypeSO.abilitiesV2 == null) {
                    companionTypeSO.abilitiesV2 = new List<EntityAbility>();
                }

                if (abilityIndex > companionTypeSO.abilitiesV2.Count) {
                    Debug.LogError("Ability index given is outside range");
                }

                if (companionTypeSO.abilitiesV2[abilityIndex].effectSteps == null) {
                    companionTypeSO.abilitiesV2[abilityIndex].effectSteps = new List<EffectStep>();
                }

                companionTypeSO.abilitiesV2[abilityIndex].effectSteps.Add(newEffect);
            }
            save(companionTypeSO);
        }

        if (GUILayout.Button("Add New Ability")) {
            if (companionTypeSO.abilitiesV2 == null) {
                companionTypeSO.abilitiesV2 = new List<EntityAbility>();
            }
            companionTypeSO.abilitiesV2.Add(new EntityAbility());
            save(companionTypeSO);
        }

        if(GUILayout.Button("Add Tooltip")) {
            companionTypeSO.tooltip = new TooltipViewModel();
            save(companionTypeSO);
        }
    }
    private void save(CompanionTypeSO companionTypeSO) {
        // These three calls cause the asset to actually be modified
        // on disc when we hit the button
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(companionTypeSO);
        AssetDatabase.SaveAssets();

    }
}

