using System.Collections;
using UnityEngine;

public enum EffectStepName {
    Default,
    GetTargets,
    SummonMinion,
    DrawCards,
    ManaChange,
    ApplyStatus,
    SelectCardsFromList,
    AddCardsToDeck,
    ConvertPlayableCardToCard,
    PermanentStatIncrease
}

[System.Serializable]
public abstract class EffectStep
{
    [HideInInspector]
    public string effectStepName;
    public virtual IEnumerator invoke(EffectDocument document) {
        yield return null;
    }
}