using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountCards : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public CountCards() {
        effectStepName = "CountCards";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.cardMap.containsValueWithKey(inputKey)) {
            EffectError("No input cards found under key " + inputKey);
            yield return null;
        }

        List<Card> cards = document.cardMap.getList(inputKey);
        int output = cards.Count;
        document.intMap[outputKey] = output;
        yield return null;
    }

}