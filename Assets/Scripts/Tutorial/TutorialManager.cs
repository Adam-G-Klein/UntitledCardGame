using System;
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


    [Header("Editor Settings")]
    //used for debugging
    [SerializeField]
    private bool shouldStartTutorialImmediate = false;
    [SerializeField]
    private string editorStartTutorial = "Intro";

    private string upcomingTutorialID;

    private TutorialLevelData tutorialLevelData;

    public static TutorialManager Instance = default;

    private HashSet<string> playedTutorials = new();

    public bool IsTutorialPlaying = false;

    private IEnumerator tutorialCoroutine;
    private bool tutorialSkipped = false;
    private TutorialAction currentAction;

    void Start()
    {
        //Not using Generic singleton because it requires use of "Instance" before it self destroys
        //manually here, also this is not supposed to be accessed global, but there should only be one
        if (Instance != default && Instance != this)
        {

            Destroy(this.gameObject);

        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(this.gameObject);

            //When a scene is loaded run this function
            SceneManager.sceneLoaded += OnSceneLoaded;

            upcomingTutorialID = FirstTutorialID;

            //do not call set up as this will start in the main menu
            //can add logic here to remove the tutorial manager once it has been completed
            if (shouldStartTutorialImmediate)
            {
                //Also replace the FirstTutorialID
                upcomingTutorialID = editorStartTutorial;
                FindTutorialInfo();
            }
        }

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsTutorialPlaying = false;
        FindTutorialInfo();
    }

    private void FindTutorialInfo()
    {
        //check to see if tutorial is being run in sequence.
        tutorialLevelData = FindObjectOfType<TutorialLevelData>();

        Debug.Log("finding tutorial level data");

        if (!tutorialSkipped && tutorialLevelData != default)
        {
            SetupNextTutorial();
        }
    }

    private void SetupNextTutorial()
    {
        currTutorial = tutorialLevelData.Get(upcomingTutorialID);

        if (currTutorial && !playedTutorials.Contains(currTutorial.ID))
        {
            tutorialCoroutine = RunTutorial();
            StartCoroutine(tutorialCoroutine);
        }
        else
        {
            Debug.Log("TUTORIAL ERROR: Unable to find specified tutorial.");
        }
    }

    private IEnumerator RunTutorial()
    {
        //set up the next tutorial incase this one is stopped mid
        upcomingTutorialID = currTutorial.nextTutorialName;
        currentStepIndex = 0;

        playedTutorials.Add(currTutorial.ID);
        IsTutorialPlaying = true;
        foreach (TutorialStep step in currTutorial.Steps)
        {
            // TODO: implement stopping the tutorial early
            currentStep = step;
            // need to invoke the steps actions here because we 
            // can't have non-monobehavior classes own coroutines
            for (currentStepIndex = 0; currentStepIndex < currentStep.actions.Count; currentStepIndex++)
            {
                Debug.LogError(currentStepIndex);
                currentAction = currentStep.actions[currentStepIndex];
                currentAction.Reset();
                yield return StartCoroutine(currentAction.Invoke());
            }
            yield return new WaitUntil(step.GetStepComplete);
            //currentStepIndex += 1;
            yield return null;
        }
        IsTutorialPlaying = false;
        DetermineNextTutorial();
    }

    public void DetermineNextTutorial()
    {
        IsTutorialPlaying = false;
        if (currTutorial.isNextTutorialSameScene)
        {
            //find the next tutorial and begin
            SetupNextTutorial();
        }
    }

    public void UnityEventStepComplete(string stepName)
    {
        if (currentStep.stepName == stepName)
        {
            //currentStep.StepComplete();
            currentStepIndex += 1;
        }
    }

    // for special casing the tutorial not starting until the player's turn in combat
    public void TurnPhaseHandler(TurnPhaseEventInfo info)
    {
        Debug.Log("Tutorial manager received turn phase event: " + info.newPhase);
        if (currentAction is WaitForTurnPhaseAction)
        {
            Debug.Log("Tutorial manager is handling a WaitForTurnPhaseAction");
            WaitForTurnPhaseAction action = (WaitForTurnPhaseAction)currentAction;
            action.OnTurnPhaseChange(info.newPhase);
        }

    }

    // for making s to skip work
    public void EndEncounterHandler()
    {
        StopCoroutine(tutorialCoroutine);
        tutorialSkipped = true;
        foreach (TutorialStep step in currTutorial.Steps)
        {
            step.SetStepComplete();
        }
    }

    //event handlers
    //need to know essentially that something has been completed, as of right now we will not need an argument, at the most potentially take in a int to reduce possible permutations
    public void EventHandle(object obj)
    {
        //cast the 

        Debug.Log("We have recieved an event from: ");
    }

    private void RemoveTutorialManager()
    {
        Debug.Log("Removing tutorial manager.");

        //Goodbye
        if (this != default)
        {
            Destroy(gameObject);
        }
    }

    public void TutorialButtonClicked() {
        Debug.Log(currentAction);
        if (currentAction != null && currentAction is WaitForNextButtonClickAction action) {
            action.ButtonClicked();
            if (currentStepIndex == currentStep.actions.Count - 1) {
                currentAction = null;
                Debug.Log("LOADING NEXT LOCATION WE ARE SO BACK");
                gameState.LoadNextLocation();
            }
        }
    }
    
    public void TutorialBackButtonClicked() {
        if (currentAction != null && currentAction is WaitForNextButtonClickAction action) {
            currentStepIndex -= 4;
            action.ButtonClicked();
        }
    }
}
