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
    public static string FirstTutorialID = "Intro";

    public GameStateVariableSO gameState;

    private TutorialData currTutorial;
    private TutorialStep currentStep;
    [SerializeField]
    private int currentStepIndex;
    [SerializeField]
    private int initStep = 0;

    private string upcomingTutorialID;

    private TutorialLevelData tutorialLevelData;

    void Start() {
        // TODO: call init on all tutorial steps
        DontDestroyOnLoad(this.gameObject);
        //When a scene is loaded run this function
        SceneManager.sceneLoaded += OnSceneLoaded;

        upcomingTutorialID = FirstTutorialID;

        //Need to manually call the set up since the sceneload event has already been fired on start up
        FindTutorialInfo();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("On Scene Load");
        FindTutorialInfo();
    }

    private void FindTutorialInfo() {
        //check to see if tutorial is being run in sequence.
        tutorialLevelData = FindObjectOfType<TutorialLevelData>();

        if (tutorialLevelData == default) {
            Debug.Log("Unable to find tutorial data in scene.");

            RemoveTutorialManager();
        }

        SetupNextTutorial();
    }

    private void SetupNextTutorial() {
        currTutorial = tutorialLevelData.Get(upcomingTutorialID);

        if (currTutorial) {
            StartCoroutine(RunTutorial());
        }
        else {
            Debug.Log("Unable to find specified tutorial.");

            RemoveTutorialManager();
        }
    }

    private IEnumerator RunTutorial() {
        //set up the next tutorial incase this one is stopped mid
        upcomingTutorialID = currTutorial.nextTutorialName;
        currentStepIndex = 0;

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

        DetermineNextTutorial();
    }

    public void DetermineNextTutorial() {
        if (currTutorial.isNextTutorialSameScene) {
            //find the next tutorial and begin
            SetupNextTutorial();
        }
    }

    public void UnityEventStepComplete(string stepName) {
        if(currentStep.stepName == stepName) {
            //currentStep.StepComplete();
            currentStepIndex += 1;
        }
    }

    //event handlers
    //need to know essentially that something has been completed, as of right now we will not need an argument, at the most potentially take in a int to reduce possible permutations
    public void EventHandle(object obj) {
        //cast the 

        Debug.Log("We have recieved an event from: ");
    }

    private void RemoveTutorialManager() {
        Debug.Log("Removing tutorial manager.");

        //Goodbye
        Destroy(gameObject);
    }
}
