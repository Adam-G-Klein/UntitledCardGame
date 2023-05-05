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
        yield return null;
    }
}