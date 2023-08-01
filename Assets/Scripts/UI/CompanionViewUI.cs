using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CompanionActionType {
    SELECT,
    VIEW_DECK,
    MOVE_COMPANION,
    COMBINE_COMPANION,
    END_COMBINE
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

    private bool isCombining;
    private HashSet<UICompanion> combiningCompanions = new HashSet<UICompanion>();
    
    private List<CompanionActionType> tempActionTypes;

    private int currentNeededToCombine;
    private void Start() {
        //This does not need to update often right now, so this will work for now
        currentNeededToCombine = CompanionUpgrader.companionsNeededToCombine;
    }

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

    private void setupButtons(bool disableButtons = true) {
        foreach (CompanionAction action in companionActions) {
            if (this.actionTypes.Contains(action.companionActionType)) {
                action.button.SetActive(true);
                action.button.GetComponent<Button>().onClick = action.buttonClickedEvent;
            } else {
                action.button.SetActive(false);
            }
        }

        if (disableButtons) {
            actionButtonsParent.SetActive(false);
        }
    }

    private void clearChildren(Transform parent) {
        //clear if necessary
        foreach (Transform child in parent) {
            Destroy(child.gameObject);
        }
    }

    private void setupActiveCompanions() {
        clearChildren(activeCompanionsParent.transform);

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
        clearChildren(benchCompanionsParent.transform);

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

        //deselect the combination companions
        foreach(var companion in combiningCompanions) {
            updateBackground(companion);
        }

        combiningCompanions.Clear();

        if (!isCombining) {
            actionButtonsParent.SetActive(false);
        }
    }

    public void companionClickedEventHandler(UICompanion uiCompanion) {
        if (isCombining) {
            handleCombiningSelection(uiCompanion);
        } else {
            handleStandardSelection(uiCompanion);
        }
    }

    private void handleStandardSelection(UICompanion uiCompanion) {
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

    private void handleCombiningSelection(UICompanion uiCompanion) {
        //determine if this companions has been clicked in this current selection and that we are not selecting more than specified number
        //deselect
        if (combiningCompanions.Contains(uiCompanion)) {
            updateBackground(uiCompanion);
            combiningCompanions.Remove(uiCompanion);
        }
        else if (combiningCompanions.Count == CompanionUpgrader.companionsNeededToCombine) {
            return;
        }
        else {
            //add it to the 
            updateBackground(uiCompanion);
            combiningCompanions.Add(uiCompanion);
        }
    }

    //Toggles the background
    private void updateBackground(UICompanion uiCompanion) {
        uiCompanion.isSelected = !uiCompanion.isSelected;

        GameObject uiCompanionSlot = getCompanionUISlot(uiCompanion);

        if (uiCompanionSlot) {
            setBackgroundSlot(uiCompanionSlot, uiCompanion.isSelected);
        }
    }

    private GameObject getCompanionUISlot(UICompanion uiCompanion) {
        int slot = companionList.IndexOf(uiCompanion.companion);

        if (slot != -1) {
            return companionSlots[slot];
        }

        slot = companionBench.IndexOf(uiCompanion.companion);

        if (slot != -1) {
            return benchCompanionSlots[slot];
        }

        Debug.LogError("Can't find companion in list, something is wrong");
        return default;
    }


    private void forceSetCompanionSelected(UICompanion uiCompanion, bool state) {
        uiCompanion.isSelected = state;

        GameObject uiCompanionSlot = getCompanionUISlot(uiCompanion);

        if (uiCompanionSlot != default) {
            if (state)
                setCompanionSlotSelected(uiCompanionSlot);
            else
                setCompanionSlotUnselected(uiCompanionSlot);
        }
    }

    private void setBackgroundSlot(GameObject companionSlot, bool state) {
        if (state)
            setCompanionSlotSelected(companionSlot);
        else
            setCompanionSlotUnselected(companionSlot);
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
        cardView.Setup(this.clickedCompanion.companion.deck.cards, 0, "");
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

    public void combineOnClick() {
        if (isCombining) {
            handleCombineAttempt();
        }
        else {
            handleCombineStart();
        }

    }

    private void handleCombineStart() {
        isCombining = true;

        //transfer the currently selected companion over to the combiningCompanions list
        combiningCompanions.Add(clickedCompanion);

        //deselect for state managment
        clickedCompanion = null;

        //Hide the other buttons and show the ones that are necessary for the combining
        //must be able to restore the old actions
        tempActionTypes = actionTypes;
        actionTypes = new List<CompanionActionType> { CompanionActionType.COMBINE_COMPANION, CompanionActionType.END_COMBINE };

        setupButtons(false);
    }

    private void handleCombineAttempt() {
        handleCombine();
    }

    public void exitCombine() {
        isCombining = false;

        actionTypes = tempActionTypes;
        foreach(var companion in combiningCompanions) {
            forceSetCompanionSelected(companion, false);
        }
        combiningCompanions.Clear();

        setupButtons();
    }

    public void handleCombine() {
        if (currentNeededToCombine == combiningCompanions.Count) {
            List<Companion> combiningCompanionsList = new();

            foreach (var UIcompanion in combiningCompanions) {
                combiningCompanionsList.Add(UIcompanion.companion);
                forceSetCompanionSelected(UIcompanion, false);
                Destroy(UIcompanion.gameObject);
            }

            combiningCompanions.Clear();

            CompanionUpgrader.Combine(companionList, companionBench, combiningCompanionsList);

            //update the U.I to Reflect the new companions
            setupActiveCompanions();
            setupBenchCompanions();
        }
    }
}
