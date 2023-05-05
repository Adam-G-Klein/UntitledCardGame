using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that causes the player to draw from one or multiple decks

    Input: One or more entities with a deck (companion or minion)
    Output: Stores the cards that were delt
    Parameters:
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class DrawCards : EffectStep
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
        List<CombatEntityWithDeckInstance> instances = 
            document.getCombatEntitiesWithDeckInstance(inputKey);
        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }
        foreach (CombatEntityWithDeckInstance instance in instances) {
            instance.dealCards(finalScale);
        }
        yield return null;
    }
}
