using System.Collections;
using UnityEngine;

public enum EffectStepName {
    Default,
    AddCardsToDeck,
    AddCardsToHand,
    ApplyStatus,
    CardInDeckEffect,
    CardInHandEffect,
    CombatEffectStep,
    ConvertPlayableCardToCard,
    DebugEffectStep,
    DrawCards,
    GetCardsFromDeck,
    GetNumberOfCardsInHand,
    GetPercentOfMaxHP,
    GetTargets,
    ManaChange,
    PermanentStatIncrease,
    SelectCardsFromList,
    SummonMinion
}

[System.Serializable]
public abstract class EffectStep
{
    [HideInInspector]
    public string effectStepName;
    public virtual IEnumerator invoke(EffectDocument document) {
        yield return null;
    }

    protected void EffectError(string errorString) {
        Debug.LogError(effectStepName + " Effect: " + errorString);
    }
}