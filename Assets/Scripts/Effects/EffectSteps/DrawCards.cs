using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that causes the player to draw from one or multiple decks

    Input: One or more entities with a deck (companion or minion)
    Output: Stores the cards that were delt (as PlayableCard)
    Parameters:
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class DrawCards : EffectStep /*, ITooltipProvider*/
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";
    // This field is useful when we have a card selection action after a draw, and we want to make sure all the cards are in hand.
    [SerializeField]
    private bool waitForDrawsToResolve = false;

    public DrawCards() {
        effectStepName = "DrawCards";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<DeckInstance> instances = document.map.GetList<DeckInstance>(inputKey);
        if (instances.Count == 0)
        {
            EffectError("No valid entities with deck under InputKey " + inputKey);
            yield return null;
        }

        Debug.Log("There are " + instances.Count + " deck instances under key " + inputKey);

        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }
        List<PlayableCard> cardsDelt = new List<PlayableCard>();
        foreach (DeckInstance instance in instances) {
            Debug.Log("Drawing " + finalScale + " cards from deck instance for companion " + instance.combatInstance.GetCompanionInstance().companion.GetName());
            yield return instance.DealCardsToPlayerHand(finalScale, cardsDelt, document.originEntityType == EntityType.Card);
        }
        // Add both versions of the cards delt to the document
        document.map.AddItems<PlayableCard>(outputKey, cardsDelt);
        cardsDelt.ForEach(playableCard => document.map.AddItem<Card>(outputKey, playableCard.card));

        if (waitForDrawsToResolve)
        {
            Debug.Log("Waiting for drawn cards to be in hand...");
            yield return new WaitUntil(() => {
                foreach (PlayableCard card in cardsDelt)
                {
                    if (!PlayerHand.Instance.ContainsPlayableCard(card))
                    {
                        return false;
                    }
                }
                return true;
            });
            // yield return new WaitForSeconds(0.3f); // Small delay to ensure all card state updates have resolved
            yield return new WaitUntil(() => LeanTween.tweensRunning == 0);
        }

        yield return null;
    }

    /*public TooltipViewModel GetTooltip() {
        return KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.Draw);
    }*/
}
