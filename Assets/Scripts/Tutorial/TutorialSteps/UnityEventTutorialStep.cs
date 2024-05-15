using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

/*
Just use a UnityEvent to call StepComplete(stringName) 
on the tutorialManager in the scene to advance through the step
*/

[System.Serializable]
public class UnityEventTutorialStep : TutorialStep {

    public UnityEventTutorialStep() {
        tutorialStepType = "UnityEventTutorialStep";
    }

}