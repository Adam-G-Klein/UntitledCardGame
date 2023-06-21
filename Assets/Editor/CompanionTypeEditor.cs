using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompanionTypeSO))]
public class CompanionTypeEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;

    public override void OnInspectorGUI() {
        CompanionTypeSO companionTypeSO = (CompanionTypeSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);

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
                if (companionTypeSO.ability == null) {
                    companionTypeSO.ability = new CompanionAbility();
                    companionTypeSO.ability.effectSteps = new List<EffectStep>();
                }
                companionTypeSO.ability.effectSteps.Add(newEffect);
            }
        }
    }

}
