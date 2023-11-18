using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CardListEventListener))]
public class TargettingManager : GenericSingleton<TargettingManager>
{
    public Action<Targetable> targetSuppliedHandler;
    public Action<CancelContext> cancelTargettingHandler;

    [SerializeField]
    private GameObject cardSelectionUIPrefab;
    [SerializeField]
    private TargettingArrowsController targettingArrows;

    // ========== Card Selecting ========== //
    private bool lookingForCardSelections;
    private List<Card> cardSelectionsList;

    public void InvokeTargetSuppliedHandler(Targetable target) {
        Debug.Log("Target supplied");
        if (targetSuppliedHandler != null) {
            targetSuppliedHandler.Invoke(target);
        }
    }

    public void InvokeCancelTargettingHandler(CancelContext context) {
        if (cancelTargettingHandler != null) {
            cancelTargettingHandler.Invoke(context);
        }
    }

    public void selectCards(
            List<Card> options,
            string promptText,
            int cardsToSelect,
            List<Card> output) {
        GameObject gameObject = GameObject.Instantiate(
            cardSelectionUIPrefab,
            Vector3.zero,
            Quaternion.identity);
        CardViewUI cardViewUI = gameObject.GetComponent<CardViewUI>();
        cardViewUI.Setup(options, cardsToSelect, promptText);
        cardSelectionsList = output;
        lookingForCardSelections = true;
    }

    public void cardsSelectedEventHandler(CardListEventInfo eventInfo) {
        if (!lookingForCardSelections) {
            Debug.LogError("TargettingManager: Cards selected event raised but" +
            " not looking for card selections!");
            return;
        }
        cardSelectionsList.AddRange(eventInfo.cards);
        lookingForCardSelections = false;
        cardSelectionsList = null;
    }
}

public class CancelContext {
    public bool canCancel;
}