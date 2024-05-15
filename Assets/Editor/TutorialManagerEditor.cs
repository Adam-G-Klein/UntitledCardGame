using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(TutorialManager))]
public class TutorialManagerEditor : Editor {

    TutorialStepName stepName = TutorialStepName.UnityEventTutorialStep;
    TutorialActionName actionName = TutorialActionName.DebugAction;
    TutorialManager manager;
    int stepToAddTo = 0;

    public override void OnInspectorGUI() {
        manager = (TutorialManager) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Tutorial Step Controls");
        EditorGUILayout.Space(5);

        stepName = (TutorialStepName) EditorGUILayout.EnumPopup(
            "New Step",
            stepName);

        if (GUILayout.Button("Add Step")) {
            TutorialStep newStep = InstantiateFromClassname.Instantiate<TutorialStep>(
                stepName.ToString(), 
                new object[] {});
            
            if(manager.tutorialSteps == null)
                manager.tutorialSteps = new List<TutorialStep>();
            manager.tutorialSteps.Add(newStep);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Tutorial Step Action Controls");
        EditorGUILayout.Space(5);

        stepToAddTo = EditorGUILayout.IntField(stepToAddTo);
        actionName = (TutorialActionName) EditorGUILayout.EnumPopup(
            "Action Type To Add",
            actionName);

        if(GUILayout.Button("Add Action")) {
            AddAction();
        }

    }

    private void AddAction(){
        if(manager.tutorialSteps == null || stepToAddTo >= manager.tutorialSteps.Count) {
            Debug.LogError("Can't find step " + stepToAddTo + " in TutorialSteps list, please add tutorial steps or decrease stepToAddTo");
            return;
        }
        TutorialStep editedStep = manager.tutorialSteps[stepToAddTo];

        TutorialAction newAction = InstantiateFromClassname.Instantiate<TutorialAction>(
            actionName.ToString(), 
            new object[] {});
        
        if(editedStep.actions == null)
            editedStep.actions = new List<TutorialAction>();
        editedStep.actions.Add(newAction);
    }

}
