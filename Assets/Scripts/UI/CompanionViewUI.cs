using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CompanionActionType {
    SELECT,
    VIEW_DECK,
    MOVE_COMPANION
}

[System.Serializable]
public class CompanionAction {
    public GameObject button;
    public Button.ButtonClickedEvent buttonClickedEvent;
    public CompanionActionType companionActionType;    
}

public class CompanionViewUI : MonoBehaviour, IPointerClickHandler
{
    public GameObject activeCompanionsParent;
    public GameObject benchCompanionsParent;
    public GameObject uICompanionPrefab;
    public GameObject actionButtonsParent;
    public GameObject deckViewUIPrefab;
    public List<CompanionAction> companionActions;
    public VoidGameEvent companionViewExitedEvent;
    public CompanionEvent companionSelectedEvent;

    private CompanionListVariableSO companionList;
    private List<CompanionActionType> actionTypes;
    private UICompanion clickedCompanion;
    private bool isCompanionClicked = false;

    public void setupCompanionDisplay(CompanionListVariableSO companionList,
            List<CompanionActionType> actionTypes) {
        this.companionList = companionList;
        this.actionTypes = actionTypes;

        setupActiveCompanions();
        setupBenchCompanions();
        setupButtons();
    }

    private void setupButtons() {
        foreach (CompanionAction action in companionActions) {
            if (this.actionTypes.Contains(action.companionActionType)) {
                action.button.SetActive(true);
                action.button.GetComponent<Button>().onClick = action.buttonClickedEvent;
            } else {
                action.button.SetActive(false);
            }
        }
        actionButtonsParent.SetActive(false);
    }

    private void setupActiveCompanions() {
        for (int i = 0; i < companionList.companionList.Count; i++) {
            setupActiveCompanion(companionList.companionList[i]);
        }
    }

    private void setupActiveCompanion(Companion companion) {
        GameObject companionImage = GameObject.Instantiate(
            uICompanionPrefab,
            Vector3.zero,
            Quaternion.identity,
            activeCompanionsParent.transform);
        // companionImage.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        UICompanion uICompanion = 
            companionImage.GetComponent<UICompanion>();
        uICompanion.companion = companion;
        uICompanion.setup();
        uICompanion.companionViewUI = this;
    }

    private void setupBenchCompanions() {
        for (int i = 0; i < companionList.companionBench.Count; i++) {
            setupBenchCompanion(companionList.companionBench[i]);
        }
    }

    private void setupBenchCompanion(Companion companion) {
        GameObject companionImage = GameObject.Instantiate(
            uICompanionPrefab,
            Vector3.zero,
            Quaternion.identity,
            benchCompanionsParent.transform);
        // companionImage.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        UICompanion uICompanion = 
            companionImage.GetComponent<UICompanion>();
        uICompanion.companion = companion;
        uICompanion.setup();
        uICompanion.companionViewUI = this;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (clickedCompanion != null) {
            clickedCompanion.toggleBackground();
            clickedCompanion = null;
        }
        actionButtonsParent.SetActive(false);
    }

    public void companionClickedEventHandler(UICompanion uiCompanion) {
        // No companion was actively clicked
        if (clickedCompanion == null) {
            clickedCompanion = uiCompanion;
            clickedCompanion.toggleBackground();
            actionButtonsParent.SetActive(true);
        }
        // Companion clicked was the last clicked companion
        else if (clickedCompanion == uiCompanion) {
            clickedCompanion.toggleBackground();
            clickedCompanion = null;
            actionButtonsParent.SetActive(false);
        }
        // Companion clicked was not the last companion clicked
        else {
            clickedCompanion.toggleBackground();
            clickedCompanion = uiCompanion;
            clickedCompanion.toggleBackground();
            actionButtonsParent.SetActive(true);
        }
    }

    public void selectButtonOnClick() {
        companionSelectedEvent.Raise(this.clickedCompanion.companion);
    }

    public void viewDeckButtonOnClick() {
        GameObject deckViewUI = GameObject.Instantiate(
                        deckViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        CardViewUI cardView = deckViewUI.GetComponent<CardViewUI>();
        cardView.Setup(this.clickedCompanion.companion.deck.cards);
    }

    public void toBenchButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        if (companionList.companionBench.Contains(companion)) {
            Debug.LogError("Can't move companion to bench that is already there!");
            return;
        }
        Destroy(this.clickedCompanion.gameObject);
        companionList.companionList.Remove(companion);
        setupBenchCompanion(companion);
        companionList.companionBench.Add(companion);
        this.clickedCompanion = null;
        actionButtonsParent.SetActive(false);
    }

    public void toActiveButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        if (companionList.companionList.Contains(companion)) {
            Debug.LogError("Can't move companion to active that is already there!");
            return;
        }
        Destroy(this.clickedCompanion.gameObject);
        companionList.companionBench.Remove(companion);
        setupActiveCompanion(companion);
        companionList.companionList.Add(companion);
        this.clickedCompanion = null;
        actionButtonsParent.SetActive(false);
    }

    public void exitView() {
        companionViewExitedEvent.Raise(null);
        Destroy(this.gameObject);
    }
}
