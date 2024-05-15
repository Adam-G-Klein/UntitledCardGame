using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* quick thought as I'm realizing we CAN use a coroutine because 
we're having this manager span scenes with DoNotDestroyOnLoad...
holy shit this list of steps and actions is gonna get massive. And super unwieldly.
Saving that UX problem for V2*/

public class TutorialManager : MonoBehaviour
{
    [SerializeReference]
    public List<TutorialStep> tutorialSteps;
    private TutorialStep currentStep;
    [SerializeField]
    private int currentStepIndex;
    [SerializeField]
    private int initStep = 0;

    void Start() {
        currentStep = tutorialSteps[initStep];
        // TODO: call init on all tutorial steps
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial() {
        foreach(TutorialStep step in tutorialSteps) {
            // TODO: implement stopping the tutorial early
            currentStep = step;
            // need to invoke the steps actions here because we 
            // can't have non-monobehavior classes own coroutines
            foreach(TutorialAction action in currentStep.actions) {
                yield return StartCoroutine(action.Invoke());
            }
            //yield return new WaitUntil(step.StepComplete());
            currentStepIndex += 1;
            yield return null;
        }
    }

    public void UnityEventStepComplete(string stepName) {
        if(currentStep.stepName == stepName) {
            //currentStep.StepComplete();
            currentStepIndex += 1;
        }
    }

}
