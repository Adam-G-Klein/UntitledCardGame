using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCardsFromList : EffectStep {
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

    public SelectCardsFromList() {
        effectStepName = "SelectCardsFromList";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        if (!document.cardMap.containsValueWithKey(inputKey)) {
            Debug.LogError("SelctCardsFromList EffectStep: InputKey " + inputKey +
                " doesn't exist in the EffectDocument");
            yield return null;
        }

        List<Card> cardOptions = document.cardMap.getList(inputKey);

        // TODO: Use the list above and activate UI card selection

        yield return null;
    }
}