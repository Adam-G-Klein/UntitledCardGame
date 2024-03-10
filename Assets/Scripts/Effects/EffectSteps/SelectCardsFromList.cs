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
    private int minTargets = 1;
    [SerializeField]
    [Header("If -1, no max number of targets")]
    private int maxTargets = -1;
    [SerializeField]
    private bool getNumberOfTargetsFromKey = false;
    [SerializeField]
    private string inputTargetsKey = "";
    [SerializeField]
    private bool randomizeOrder = false;

    public SelectCardsFromList() {
        effectStepName = "SelectCardsFromList";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        if (!document.map.ContainsValueWithKey<Card>(inputKey)) {
            EffectError("InputKey " + inputKey + " doesn't exist in the EffectDocument");
            yield return null;
        }

        List<Card> cardOptions = document.map.GetList<Card>(inputKey);
        if (randomizeOrder) {
            cardOptions = ShuffleCards(new List<Card>(cardOptions));
        }
        List<Card> selections = new List<Card>();

        TargettingManager.Instance.selectCards(cardOptions, promptText, minTargets, maxTargets, selections);
        yield return new WaitUntil(() => selections.Count > 0);

        document.map.AddItems<Card>(outputKey, selections);

        yield return null;
    }

    private List<Card> ShuffleCards(List<Card> cards) {
        System.Random _random = new System.Random();
        Card temp;

        int n = cards.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1
            int r = i + (int)(_random.NextDouble() * (n - i));
            temp = cards[r];
            cards[r] = cards[i];
            cards[i] = temp;
        }
        return cards;
    }
}