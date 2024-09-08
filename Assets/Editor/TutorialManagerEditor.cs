using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing.Printing;

[CustomEditor(typeof(TutorialData))]
public class TutorialManagerEditor : Editor {

    TutorialStepName stepName = TutorialStepName.UnityEventTutorialStep;
    TutorialActionName actionName = TutorialActionName.DebugAction;
    TutorialData manager;
    int stepToAddTo = 0;

    public override void OnInspectorGUI() {
        manager = (TutorialData) target;
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
            
            if(manager.Steps == null)
                manager.Steps = new List<TutorialStep>();
            manager.Steps.Add(newStep);
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
        if(manager.Steps == null || stepToAddTo >= manager.Steps.Count) {
            Debug.LogError("Can't find step " + stepToAddTo + " in TutorialSteps list, please add tutorial steps or decrease stepToAddTo");
            return;
        }
        TutorialStep editedStep = manager.Steps[stepToAddTo];

        TutorialAction newAction = InstantiateFromClassname.Instantiate<TutorialAction>(
            actionName.ToString(), 
            new object[] {});
        
        if(editedStep.actions == null)
            editedStep.actions = new List<TutorialAction>();
        editedStep.actions.Add(newAction);
    }

}
