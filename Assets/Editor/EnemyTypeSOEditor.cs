using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyTypeSO))]
public class EnemyTypeSOEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    int index = 0;

    public override void OnInspectorGUI() {
        EnemyTypeSO enemyType = (EnemyTypeSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);
        index = EditorGUILayout.IntField("Action Index", index);
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
                enemyType.enemyPattern.behaviors[index].effectSteps.Add(newEffect);
            }
        }
    }
}