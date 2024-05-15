using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

/*
Simplest step type. Just goes right through its actions
*/

[System.Serializable]
public class NoAdvancementCriteriaStep : TutorialStep {

    public NoAdvancementCriteriaStep() {
        tutorialStepType = "NoAdvancementCriteriaStep";
    }

}