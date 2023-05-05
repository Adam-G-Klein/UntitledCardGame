using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestEffects))]
public class TestEffectsEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    public override void OnInspectorGUI() {
        TestEffects testEffects = (TestEffects) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Procedure Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);

        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(), 
                new object[] {});

            if(newEffect == null) {
                Debug.LogError("Failed to instantiate effect procedure, " +
                "please check Scripts/Cards/CardEffectProcedures/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else {
                testEffects.effects.Add(newEffect);
            }
        }
    }

}
