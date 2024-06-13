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

        if (GUILayout.Button("Add Effect")) {
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
                if (companionTypeSO.abilities == null) {
                    companionTypeSO.abilities = new List<CompanionAbility>();
                }
                
                if (abilityIndex > companionTypeSO.abilities.Count) {
                    Debug.LogError("Ability index given is outside range");
                }

                if (companionTypeSO.abilities[abilityIndex].effectSteps == null) {
                    companionTypeSO.abilities[abilityIndex].effectSteps = new List<EffectStep>();
                }

                 companionTypeSO.abilities[abilityIndex].effectSteps.Add(newEffect);
            }
        }

        if (GUILayout.Button("Add New Ability")) {
            if (companionTypeSO.abilities == null) {
                companionTypeSO.abilities = new List<CompanionAbility>();
            }
            companionTypeSO.abilities.Add(new CompanionAbility());
        }

        if(GUILayout.Button("Add Tooltip")) {
            companionTypeSO.tooltip = new Tooltip();
        }
    }
}
