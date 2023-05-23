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
    public GameObject companionSlotsParent;
    public GameObject benchCompanionsParent;
    public GameObject benchCompanionSlotsParent;
    public GameObject uICompanionPrefab;
    public GameObject uICompanionSlotPrefab;
    public Color companionSlotUnselectedColor;
    public Color companionSlotSelectedColor;
    public GameObject actionButtonsParent;
    public GameObject deckViewUIPrefab;
    public List<CompanionAction> companionActions;
    public VoidGameEvent companionViewExitedEvent;
    public CompanionEvent companionSelectedEvent;
    public int NUMBER_OF_BENCH_COMPANION_SLOTS;

    private List<Companion> companionList;
    private List<Companion> companionBench;
    private int currentCompanionSlots;

    private List<CompanionActionType> actionTypes;
    private UICompanion clickedCompanion;
    private List<GameObject> companionSlots = new List<GameObject>();
    private List<GameObject> benchCompanionSlots = new List<GameObject>();

    public void setupCompanionDisplay(CompanionListVariableSO companionListVariableSO,
        List<CompanionActionType> actionTypes) {
        //Since this is an SO, copy the refernces for later use
        this.companionList = companionListVariableSO.companionList;
        this.companionBench = companionListVariableSO.companionBench;
        this.currentCompanionSlots = companionListVariableSO.currentCompanionSlots;

        this.actionTypes = actionTypes;

        setupCompanionDisplayHelper();
    }

    //This is for when you want to display a modified companion set
    public void setupCompanionDisplay(List<Companion> companionList, List<Companion> companionBench, int currentCompanionSlots,
            List<CompanionActionType> actionTypes) {
        this.companionList = companionList;
        this.companionBench = companionBench;
        this.currentCompanionSlots = currentCompanionSlots;
        this.actionTypes = actionTypes;

        setupCompanionDisplayHelper();
    }

    private void setupCompanionDisplayHelper() {
        setupActiveCompanions();
        setupCompanionSlots();
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
        for (int i = 0; i < companionList.Count; i++) {
            setupActiveCompanion(companionList[i]);
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

    private void setupCompanionSlots() {
        for (int i = 0; i < currentCompanionSlots; i++) {
            GameObject companionSlot = GameObject.Instantiate(
                uICompanionSlotPrefab,
                Vector3.zero,
                Quaternion.identity,
                companionSlotsParent.transform);
            setCompanionSlotUnselected(companionSlot);
            companionSlots.Add(companionSlot);
        }
        for (int i = 0; i < NUMBER_OF_BENCH_COMPANION_SLOTS; i++) {
            GameObject companionSlot = GameObject.Instantiate(
                uICompanionSlotPrefab,
                Vector3.zero,
                Quaternion.identity,
                benchCompanionSlotsParent.transform);
            setCompanionSlotUnselected(companionSlot);
            benchCompanionSlots.Add(companionSlot);
        }
    }

    private void setupBenchCompanions() {
        for (int i = 0; i < companionBench.Count; i++) {
            setupBenchCompanion(companionBench[i]);
        }
    }

    private void setupBenchCompanion(Companion companion) {
        GameObject companionImage = GameObject.Instantiate(
            uICompanionPrefab,
            Vector3.zero,
            Quaternion.identity,
            benchCompanionsParent.transform);
        UICompanion uICompanion = 
            companionImage.GetComponent<UICompanion>();
        uICompanion.companion = companion;
        uICompanion.setup();
        uICompanion.companionViewUI = this;
    }

    private void setCompanionSlotSelected(GameObject companionSlot) {
        companionSlot.GetComponent<Image>().color = companionSlotSelectedColor;
    }

    private void setCompanionSlotUnselected(GameObject companionSlot) {
        companionSlot.GetComponent<Image>().color = companionSlotUnselectedColor;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (clickedCompanion != null) {
            updateBackground(clickedCompanion);
            clickedCompanion = null;
        }
        actionButtonsParent.SetActive(false);
    }

    public void companionClickedEventHandler(UICompanion uiCompanion) {
        // No companion was actively clicked
        if (clickedCompanion == null) {
            clickedCompanion = uiCompanion;
            updateBackground(clickedCompanion);
            actionButtonsParent.SetActive(true);
        }
        // Companion clicked was the last clicked companion
        else if (clickedCompanion == uiCompanion) {
            updateBackground(clickedCompanion);
            clickedCompanion = null;
            actionButtonsParent.SetActive(false);
        }
        // Companion clicked was not the last companion clicked
        else {
            updateBackground(clickedCompanion);
            clickedCompanion = uiCompanion;
            updateBackground(clickedCompanion);
            actionButtonsParent.SetActive(true);
        }
    }

    private void updateBackground(UICompanion uiCompanion) {
        int i = 0;
        i = companionList.IndexOf(uiCompanion.companion);
        if ( i != -1) {
            uiCompanion.isSelected = !uiCompanion.isSelected;
            setBackgroundForActiveSlot(i, uiCompanion.isSelected);
            return;
        }

        i = companionBench.IndexOf(uiCompanion.companion);
        if (i == -1) {
            Debug.LogError("Can't find companion in list, something is wrong");
            return;
        }
        
        uiCompanion.isSelected = !uiCompanion.isSelected;
        setBackgroundForBenchSlot(i, uiCompanion.isSelected);
    }


    private void setBackgroundForActiveSlot(int slotNumber, bool state) {
        GameObject slot = companionSlots[slotNumber];
        if (state)
            setCompanionSlotSelected(slot);
        else
            setCompanionSlotUnselected(slot);
    }

    private void setBackgroundForBenchSlot(int slotNumber, bool state) {
        GameObject slot = benchCompanionSlots[slotNumber];
        if (state)
            setCompanionSlotSelected(slot);
        else
            setCompanionSlotUnselected(slot);
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
        if (companionBench.Contains(companion)) {
            Debug.LogError("Can't move companion to bench that is already there!");
            return;
        }
        updateBackground(this.clickedCompanion);
        Destroy(this.clickedCompanion.gameObject);
        companionList.Remove(companion);
        setupBenchCompanion(companion);
        companionBench.Add(companion);
        this.clickedCompanion = null;
        actionButtonsParent.SetActive(false);
    }

    public void toActiveButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        if (companionList.Contains(companion)) {
            Debug.LogError("Can't move companion to active that is already there!");
            return;
        }
        if (companionList.Count == currentCompanionSlots) {
            return;
        }
        updateBackground(this.clickedCompanion);
        Destroy(this.clickedCompanion.gameObject);
        companionBench.Remove(companion);
        setupActiveCompanion(companion);
        companionList.Add(companion);
        this.clickedCompanion = null;
        actionButtonsParent.SetActive(false);
    }

    public void exitView() {
        companionViewExitedEvent.Raise(null);
        Destroy(this.gameObject);
    }
}
