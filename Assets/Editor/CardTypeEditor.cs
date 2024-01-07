using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGraph;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(CardType))]
public class CardTypeEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    int editedEffectWorkflow = 0;
    public override void OnInspectorGUI() {
        CardType cardType = (CardType) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Effect Step Controls");
        EditorGUILayout.Space(5);

        stepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            stepName);
        editedEffectWorkflow = EditorGUILayout.IntField(editedEffectWorkflow);

        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                stepName.ToString(), 
                new object[] {});
            
            EffectWorkflow retrievedWorkflow = null;
            if (cardType.effectWorkflows.Count == 0) {
                cardType.effectWorkflows.Add(new EffectWorkflow());
            }
            if(cardType.effectWorkflows.Count < editedEffectWorkflow) {
                Debug.LogError("Attempting to edit an effect workflow that does not exist. Please check " +
                "the amount of effect workflows you have in the list, and the zero-indexed value you have in " +
                "editedEffectWorkflow");
            } else {
                retrievedWorkflow = cardType.effectWorkflows[editedEffectWorkflow];
            }
            
            if(retrievedWorkflow == null) {
                Debug.LogError("Error while attempting to retrieve the workflow to add a new effect to");
            }

            if(newEffect == null) {
                Debug.LogError("Failed to instantiate effect step, " +
                "please check Scripts/Effects/EffectSteps/* to verify the className for the  " +
                " and verify that the arguments set in the editor correspond to " +
                " the arguments in the constructor");
            }
            else {
                retrievedWorkflow.effectSteps.Add(newEffect);
            }
            save(cardType);

        }

        if (GUILayout.Button("Add EffectWorkflow")) {
            cardType.effectWorkflows.Add(new EffectWorkflow());
            save(cardType);
        }

    }

    private void save(CardType cardType) {
        // These three calls cause the asset to actually be modified
        // on disc when we hit the button
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(cardType);
        AssetDatabase.SaveAssets();

    }

}
