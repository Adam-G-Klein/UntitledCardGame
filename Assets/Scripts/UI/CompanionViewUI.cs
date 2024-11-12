using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using Unity.VisualScripting;

public enum CompanionActionType {
    SELECT,
    VIEW_DECK,
    MOVE_COMPANION,
    COMBINE_COMPANION,
    END_COMBINE,
    SELL,
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
    public GameObject sellConfirmationPrefab;
    public Color companionSlotUnselectedColor;
    public Color companionSlotSelectedColor;
    public Color companionSlotUnavailableColor;
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

    // playerData is used to manipulate the player's gold.
    private PlayerDataVariableSO playerData;

    private ShopDataSO shopDataSO;

    private List<CompanionActionType> tempActionTypes;
    private int currentNeededToCombine;

    // New fields
    private List<GameObject> activeCompanionGameObjectList;
    private List<GameObject> benchedCompanionGameObjectList;
    public GameObject activeCompanionSlotsParent;
    public GameObject benchedCompanionSlotsParent;

    private void Start() {
        //This does not need to update often right now, so this will work for now
        currentNeededToCombine = CompanionUpgrader.companionsNeededToCombine;
    }

    public void setupCompanionDisplay(
        CompanionListVariableSO companionListVariableSO,
        List<CompanionActionType> actionTypes,
        PlayerDataVariableSO playerData,
        ShopDataSO shopData
    ) {
        //Since this is an SO, copy the refernces for later use
        this.companionList = companionListVariableSO.activeCompanions;
        this.companionBench = companionListVariableSO.benchedCompanions;
        this.currentCompanionSlots = companionListVariableSO.currentCompanionSlots;

        this.actionTypes = actionTypes;

        this.playerData = playerData;
        this.shopDataSO = shopData;

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
        this.activeCompanionGameObjectList = new List<GameObject>();
        this.benchedCompanionGameObjectList = new List<GameObject>();
        setupActiveCompanions();
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
            // actionButtonsParent.SetActive(false);
            SetButtonsNotInteractable();
        }
    }

    private void setupActiveCompanions() {
        for (int i = 0; i < companionList.Count; i++) {
            Transform parentTransform = this.activeCompanionSlotsParent.transform.GetChild(i);
            setupActiveCompanion(companionList[i], parentTransform);
        }
        for (int i = 0; i < this.activeCompanionSlotsParent.transform.childCount; i++) {
            if (i >= this.activeCompanionGameObjectList.Count) {
                this.activeCompanionSlotsParent.transform.GetChild(i).Find("Square").gameObject.GetComponent<Image>().color = companionSlotUnavailableColor;
            }
        }
    }

    private void setupActiveCompanion(Companion companion, Transform parentTransform) {
        GameObject companionImage = GameObject.Instantiate(
            uICompanionPrefab,
            Vector3.zero,
            Quaternion.identity,
            parentTransform);
        UICompanion uICompanion =
            companionImage.GetComponent<UICompanion>();
        uICompanion.companion = companion;
        uICompanion.setup();
        uICompanion.companionViewUI = this;
        this.activeCompanionGameObjectList.Add(uICompanion.gameObject);
        uICompanion.transform.position = parentTransform.position;
    } // TODO: Still need to fix all the buttons, and I think I'm creating too many active companion slots?

    private void setupBenchCompanions() {
        for (int i = 0; i < NUMBER_OF_BENCH_COMPANION_SLOTS; i++) {
            GameObject companionSlot = GameObject.Instantiate(
                uICompanionSlotPrefab,
                Vector3.zero,
                Quaternion.identity,
                benchedCompanionSlotsParent.transform);
            setCompanionSlotUnselected(companionSlot);
        }

        for (int i = 0; i < companionBench.Count; i++) {
            Transform parentTransform = this.benchedCompanionSlotsParent.transform.GetChild(i);
            setupBenchCompanion(companionBench[i], parentTransform);
        }
    }

    private void setupBenchCompanion(Companion companion, Transform parentTransform) {
        GameObject companionImage = GameObject.Instantiate(
            uICompanionPrefab,
            Vector3.zero,
            Quaternion.identity,
            parentTransform);
        UICompanion uICompanion =
            companionImage.GetComponent<UICompanion>();
        uICompanion.companion = companion;
        uICompanion.setup();
        uICompanion.companionViewUI = this;
        this.benchedCompanionGameObjectList.Add(uICompanion.gameObject);
        uICompanion.transform.position = parentTransform.position;
    }

    private void setCompanionSlotSelected(GameObject companionSlot) {
        companionSlot.transform.Find("Square").gameObject.GetComponent<Image>().color = companionSlotSelectedColor;

    }

    private void setCompanionSlotUnselected(GameObject companionSlot) {
        companionSlot.transform.Find("Square").gameObject.GetComponent<Image>().color = companionSlotUnselectedColor;
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
            // actionButtonsParent.SetActive(false);
            SetButtonsNotInteractable();
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
            // actionButtonsParent.SetActive(true);
            SetButtonsInteractable();
        }
        // Companion clicked was the last clicked companion
        else if (clickedCompanion == uiCompanion) {
            updateBackground(clickedCompanion);
            clickedCompanion = null;

            // actionButtonsParent.SetActive(false);
            SetButtonsNotInteractable();
        }
        // Companion clicked was not the last companion clicked
        else {
            updateBackground(clickedCompanion);
            clickedCompanion = uiCompanion;

            updateBackground(clickedCompanion);
            // actionButtonsParent.SetActive(true);
            SetButtonsInteractable();
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

        GameObject uiCompanionSlot = uiCompanion.transform.parent.gameObject;

        if (uiCompanionSlot) {
            setBackgroundSlot(uiCompanionSlot, uiCompanion.isSelected);
        }
    }


    private void forceSetCompanionSelected(UICompanion uiCompanion, bool state) {
        uiCompanion.isSelected = state;

        GameObject uiCompanionSlot = uiCompanion.transform.parent.gameObject;

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
        cardView.Setup(this.clickedCompanion.companion.deck.cards, 0, "", 0);
    }

    public void toBenchButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        UICompanion uICompanion = this.clickedCompanion;
        if (companionBench.Contains(companion)) {
            Debug.LogError("Can't move companion to bench that is already there!");
            return;
        }
        if (companionList.Count == 1) {
            return;
        }
        Transform benchSlot = FindNextBenchCompanionSlot();
        if (benchSlot == null) {

        }
        // this Add call has to go after the previous line
        this.benchedCompanionGameObjectList.Add(uICompanion.gameObject);
        this.activeCompanionGameObjectList.Remove(uICompanion.gameObject);
        updateBackground(this.clickedCompanion);
        companionList.Remove(companion);
        companionBench.Add(companion);
        this.clickedCompanion = null;

        uICompanion.transform.parent = this.transform;

        IEnumerator coroutine = MoveCompanion(
            uICompanion.transform.position,
            benchSlot.position,
            uICompanion,
            benchSlot);
        StartCoroutine(coroutine);
        UpdateCompanionSlots(this.activeCompanionGameObjectList, this.activeCompanionSlotsParent);
        SetButtonsNotInteractable();
    }

    public void toActiveButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        UICompanion uICompanion = this.clickedCompanion;
        if (companionList.Contains(companion)) {
            Debug.LogError("Can't move companion to active that is already there!");
            return;
        }
        if (companionList.Count == currentCompanionSlots) {
            return;
        }
        Transform activeSlot = FindNextActiveCompanionSlot();
        if (activeSlot == null) {
            return;
        }
        // this Add call has to go after the previous line
        this.activeCompanionGameObjectList.Add(uICompanion.gameObject);
        this.benchedCompanionGameObjectList.Remove(uICompanion.gameObject);
        updateBackground(this.clickedCompanion);
        companionBench.Remove(companion);
        companionList.Add(companion);
        this.clickedCompanion = null;

        uICompanion.transform.parent = this.transform;

        IEnumerator coroutine = MoveCompanion(
            uICompanion.transform.position,
            activeSlot.position,
            uICompanion,
            activeSlot);
        StartCoroutine(coroutine);
        UpdateCompanionSlots(this.benchedCompanionGameObjectList, this.benchedCompanionSlotsParent);
        SetButtonsNotInteractable();
    }

    private void UpdateCompanionSlots(List<GameObject> companionGOList, GameObject slotsParent) {
        for (int i = 0; i < slotsParent.transform.childCount; i++) {
            if (i > companionGOList.Count) {
                return;
            }
            GameObject companionGO = companionGOList[i];
            Transform slotGO = slotsParent.transform.GetChild(i);
            if (slotGO.childCount == 2) {
                Debug.Log("Updating companion slots");
                UICompanion uICompanion = companionGO.GetComponent<UICompanion>();
                uICompanion.transform.parent = this.transform;
                IEnumerator coroutine = MoveCompanion(
                    uICompanion.transform.position,
                    slotGO.transform.position,
                    uICompanion,
                    slotGO.transform);
                StartCoroutine(coroutine);
            }
        }
    }

    public IEnumerator MoveCompanion(
            Vector3 startPosition,
            Vector3 endPosition,
            UICompanion companion,
            Transform finalParent) {
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration) {
            float evaluation = elapsedTime / duration;
            companion.transform.position = Vector3.Lerp(startPosition, endPosition, evaluation);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        companion.transform.SetParent(finalParent);
    }

    private Transform FindNextActiveCompanionSlot() {
        if (this.activeCompanionSlotsParent.transform.childCount == this.activeCompanionGameObjectList.Count) {
            return null;
        }
        return this.activeCompanionSlotsParent.transform.GetChild(this.activeCompanionGameObjectList.Count);
    }

    private Transform FindNextBenchCompanionSlot() {
        if (this.benchedCompanionSlotsParent.transform.childCount == this.benchedCompanionGameObjectList.Count) {
            return null;
        }
        return this.benchedCompanionSlotsParent.transform.GetChild(this.benchedCompanionGameObjectList.Count);
    }

    public void sellButtonOnClick() {
        Companion companion = this.clickedCompanion.companion;
        updateBackground(this.clickedCompanion);

        // Calculate the sell value with the following formula (assuming integer division).
        // Sell value = (companion price) / 2 + (num cards in deck / discount constant) * (card price)
        // We want the player to recoup some of the value if they put a lot of cards on a single companion.
        int sellValue = shopDataSO.companionKeepsakePrice / 2 + companion.deck.cards.Count / 5 * shopDataSO.cardPrice;
        playerData.GetValue().gold += sellValue;

        GameObject sellSign = Instantiate(
            sellConfirmationPrefab,
            this.gameObject.transform
        );
        TMP_Text moneyText = sellSign.GetComponent<TMP_Text>();
        moneyText.text = "+$" + sellValue.ToString();
        StartCoroutine(removeSellConfirmationSign(sellSign));

        Destroy(this.clickedCompanion.gameObject);
        // Note: Remove does not throw an Exception if the list does not contain it.
        companionList.Remove(companion);
        companionBench.Remove(companion);

        Debug.Log("Sold the companion " + companion.companionType.name);

        this.clickedCompanion = null;
        // actionButtonsParent.SetActive(false);
        SetButtonsNotInteractable();
    }

    private void SetButtonsNotInteractable() {
        foreach (CompanionAction companionAction in this.companionActions) {
            Button button = companionAction.button.GetComponent<Button>();
            button.interactable = false;
        }
    }

    private void SetButtonsInteractable() {
        foreach (CompanionAction companionAction in this.companionActions) {
            Button button = companionAction.button.GetComponent<Button>();
            button.interactable = true;
        }
    }

    private IEnumerator removeSellConfirmationSign(GameObject sellSign) {
        yield return new WaitForSeconds(3);

        Destroy(sellSign);
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
