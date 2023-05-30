using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that opens a UI with a list of cards, in which the player will have
    to select one or more of the cards.

    Input: List of cards to display (Card model, not PlayableCard)
    Output: The card(s) selected
    Parameters:
        - Targets: The number of cards to have the player choose
        - GetNumberOfTargetsFromKey: If checked, the number of targets will be pulled from a previous step
        - InputTargetsKey: The key from which to pull the targets integer from
*/
public class SelectCardsFromList : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    [TextArea]
    private string promptText = "";
    [SerializeField]
    private int targets;
    [SerializeField]
    private bool getNumberOfTargetsFromKey = false;
    [SerializeField]
    private string inputTargetsKey = "";

    public SelectCardsFromList() {
        effectStepName = "SelectCardsFromList";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        if (!document.cardMap.containsValueWithKey(inputKey)) {
            EffectError("InputKey " + inputKey + " doesn't exist in the EffectDocument");
            yield return null;
        }

        List<Card> cardOptions = document.cardMap.getList(inputKey);
        List<Card> selections = new List<Card>();

        TargettingManager.Instance.selectCards(cardOptions, promptText, targets, selections);
        yield return new WaitUntil(() => selections.Count > 0);
        Debug.Log("Test");

        document.cardMap.addItems(outputKey, selections);

        yield return null;
    }
}