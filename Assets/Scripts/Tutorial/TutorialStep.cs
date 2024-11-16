using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TutorialStepName 
{
    UnityEventTutorialStep,
    NoAdvancementCriteriaStep,
    DoNotAdvanceStep
}

/* The base class for the different types of steps we can have.
A different step implementation will have different criteria for step completion,
or potentially a different ordering or stepthrough method for its TutorialActions
*/
[System.Serializable]
public abstract class TutorialStep
{
    [HideInInspector]
    public string tutorialStepType;
    public string stepName = "gimme a name!";
    private TutorialManager manager;
    [SerializeReference]
    public List<TutorialAction> actions;
    private TutorialAction currentAction;
    [SerializeField]
    private int initAction = 0;
    private bool stepComplete = true;

    public virtual void Init(TutorialManager manager) {
        this.manager = manager;
        if(actions != null) {
            currentAction = actions[initAction];
        } else {
            Debug.LogError("TutorialStep " + stepName + " has no actions. Add one using the TutorialManager");
        }
    }

    public virtual bool GetStepComplete() {
        return stepComplete;
    }

    // can we do this with a Broadcast? 
    public virtual void SetStepComplete() {
        stepComplete = true;
    } 

    protected void StepError(string errorString) {
        Debug.LogError(tutorialStepType + " TutorialStep: " + errorString);
    }

}
