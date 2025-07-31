using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(EffectStepSO))]
public class EffectStepSOEditor : Editor {
    EffectStepName stepName = EffectStepName.Default;

    public override void OnInspectorGUI() {
        EffectStepSO effectStepSO = (EffectStepSO) target;
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
            effectStepSO.effectStep = newEffect;
        }
    }
}