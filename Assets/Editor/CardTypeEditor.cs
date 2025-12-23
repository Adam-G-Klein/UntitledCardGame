using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(CardType))]
public class CardTypeEditor : Editor {

    EffectStepName stepName = EffectStepName.Default;
    EffectStepName onExhaustStepName = EffectStepName.Default;
    EffectStepName onDiscardStepName = EffectStepName.Default;

    EffectStepName inPlayerHandEndOfTurnStepName = EffectStepName.Default;
    EffectStepName onDrawStepName = EffectStepName.Default;

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

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("On Exhaust effect Step Controls");
        EditorGUILayout.Space(5);

        onExhaustStepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            onExhaustStepName);

        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                onExhaustStepName.ToString(),
                new object[] {});
            if (cardType.onExhaustEffectWorkflow == null) {
                cardType.onExhaustEffectWorkflow = new EffectWorkflow();
            }

            cardType.onExhaustEffectWorkflow.effectSteps.Add(newEffect);
            save(cardType);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("On discard effect Step Controls");
        EditorGUILayout.Space(5);

        onDiscardStepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            onDiscardStepName);

        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                onDiscardStepName.ToString(),
                new object[] {});
            if (cardType.onDiscardEffectWorkflow == null) {
                cardType.onDiscardEffectWorkflow = new EffectWorkflow();
            }

            cardType.onDiscardEffectWorkflow.effectSteps.Add(newEffect);
            save(cardType);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("In Player hand end of turn effect step controls");
        EditorGUILayout.Space(5);

        inPlayerHandEndOfTurnStepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            inPlayerHandEndOfTurnStepName);

        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                inPlayerHandEndOfTurnStepName.ToString(),
                new object[] {});
            if (cardType.inPlayerHandEndOfTurnWorkflow == null) {
                cardType.inPlayerHandEndOfTurnWorkflow = new EffectWorkflow();
            }

            cardType.inPlayerHandEndOfTurnWorkflow.effectSteps.Add(newEffect);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("On Draw effect Step Controls");
        EditorGUILayout.Space(5);

        onDrawStepName = (EffectStepName) EditorGUILayout.EnumPopup(
            "New effect",
            onDrawStepName);
        if (GUILayout.Button("Add Effect")) {
            EffectStep newEffect = InstantiateFromClassname.Instantiate<EffectStep>(
                onDrawStepName.ToString(),
                new object[] {});
            if (cardType.onDrawEffectWorkflow == null) {
                cardType.onDrawEffectWorkflow = new EffectWorkflow();
            }
            cardType.onDrawEffectWorkflow.effectSteps.Add(newEffect);
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
