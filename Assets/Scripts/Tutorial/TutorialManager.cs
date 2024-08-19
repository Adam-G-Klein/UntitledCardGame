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

    //used for debugging
    [SerializeField]
    private bool shouldStartTutorialImmediate = false;

    private string upcomingTutorialID;

    private TutorialLevelData tutorialLevelData;

    private static TutorialManager instance = default;

    void Start() {
        //Not using Generic singleton because it requires use of "Instance" before it self destroys
        //manually here, also this is not supposed to be accessed global, but there should only be one
        if (instance != default && instance != this) {
            
            Destroy(this.gameObject);
            
        }
        instance = this;

        DontDestroyOnLoad(this.gameObject);
        //When a scene is loaded run this function
        SceneManager.sceneLoaded += OnSceneLoaded;

        upcomingTutorialID = FirstTutorialID;

        //do not call set up as this will start in the main menu
        //can add logic here to remove the tutorial manager once it has been completed
        if (shouldStartTutorialImmediate) {
            FindTutorialInfo();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        FindTutorialInfo();
    }

    private void FindTutorialInfo() {
        //check to see if tutorial is being run in sequence.
        tutorialLevelData = FindObjectOfType<TutorialLevelData>();

        Debug.Log("finding tutorial level data");

        if (tutorialLevelData != default) {
            SetupNextTutorial();
        }
    }

    private void SetupNextTutorial() {
        currTutorial = tutorialLevelData.Get(upcomingTutorialID);

        if (currTutorial) {
            StartCoroutine(RunTutorial());
        }
        else {
            //Debug.Log("Unable to find specified tutorial.");
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
        if (this != default) {
            Destroy(gameObject);
        }
    }
}
