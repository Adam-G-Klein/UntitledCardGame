using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* quick thought as I'm realizing we CAN use a coroutine because 
we're having this manager span scenes with DoNotDestroyOnLoad...
holy shit this list of steps and actions is gonna get massive. And super unwieldly.
Saving that UX problem for V2*/

public class TutorialManager : MonoBehaviour
{
    public GameStateVariableSO gameState;

    private TutorialData currTutorial;
    private TutorialStep currentStep;
    [SerializeField]
    private int currentStepIndex;
    [SerializeField]
    private int initStep = 0;

    void Start() {
        // TODO: call init on all tutorial steps

        //reset the tutorials for now lol, is there a reset somewhere?
        gameState.currentTutorial = 0;

        DontDestroyOnLoad(this.gameObject);
        //When a scene is loaded run this function
        SceneManager.sceneLoaded += OnSceneLoaded;

        //Need to manually call the set up since the sceneload event has already been fired
        SetUp();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        SetUp();
    }

    private void SetUp() {
        //Find the tutorialInformation
        TutorialLevelData data = GameObject.FindObjectOfType<TutorialLevelData>();

        if (data == default) {
            Debug.Log("Unable to find Tutorial Data in Scene.");

            //Goodbye
            Destroy(gameObject);
        }

        currTutorial = data.Get(gameState.currentTutorial);

        if (currTutorial) {
            StartCoroutine(RunTutorial());
        }
        else {
            Debug.Log("Unable to find specified tutorial");

            //Goodbye
            Destroy(gameObject);
        }
    }

    private IEnumerator RunTutorial() {
        foreach(TutorialStep step in currTutorial.Steps) {
            // TODO: implement stopping the tutorial early
            currentStep = step;
            // need to invoke the steps actions here because we 
            // can't have non-monobehavior classes own coroutines
            foreach(TutorialAction action in currentStep.actions) {
                yield return StartCoroutine(action.Invoke());
            }
            yield return new WaitUntil(step.GetStepComplete);
            currentStepIndex += 1;
            yield return null;
        }

        //advance to the next tutorial, in the next scene
        gameState.currentTutorial++;
    }

    public void UnityEventStepComplete(string stepName) {
        if(currentStep.stepName == stepName) {
            //currentStep.StepComplete();
            currentStepIndex += 1;
        }
    }

}
