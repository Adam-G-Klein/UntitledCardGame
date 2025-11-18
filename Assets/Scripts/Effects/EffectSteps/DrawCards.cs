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

        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }
        List<PlayableCard> cardsDelt = new List<PlayableCard>();
        foreach (DeckInstance instance in instances) {
            cardsDelt.AddRange(instance.DealCardsToPlayerHand(finalScale, document.originEntityType == EntityType.Card));
        }
        // Add both versions of the cards delt to the document
        document.map.AddItems<PlayableCard>(outputKey, cardsDelt);
        cardsDelt.ForEach(playableCard => document.map.AddItem<Card>(outputKey, playableCard.card));
        yield return null;
    }

    /*public TooltipViewModel GetTooltip() {
        return KeywordTooltipProvider.Instance.GetTooltip(TooltipKeyword.Draw);
    }*/
}
