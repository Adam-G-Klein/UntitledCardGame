using System.Collections;
using UnityEngine;

public enum EffectStepName {
    AddCardsToDeck,
    AddCardsToHand,
    AddManaNextTurn,
    ApplyStatus,
    BooleanComparison,
    CardInDeckEffect,
    CardInHandEffect,
    CardModificationEffect,
    CheckTypeOfMostRecentlyPlayedCard,
    CombatEffectStep,
    ConvertPlayableCardToCard,
    CountStatusEffectsStep,
    CountStep,
    DebugEffectStep,
    Default,
    DrawCards,
    EndWorkflowIfConditionMet,
    EndWorkflowIfNoMapElement,
    FilterByCardCategory,
    FilterEntityByStatus,
    GetAdjacentCompanions,
    GetCardsFromDeck,
    GetCardsFromDiscard,
    GetCastCountFromCard,
    GetDecksShuffledThisCombat,
    GetNumberOfCardsInHand,
    GetPercentOfMaxHP,
    GetTargets,
    GoldManipulation,
    HasCardActionBeenTakenThisTurn,
    InstantiatePrefab,
    ManaChange,
    MathStep,
    PermanentStatIncrease,
    QuantityOfCardTypePlayedThisTurn,
    RemoveStatus,
    SelectCardsFromList,
    SetCardEffectWorkflow,
    SummonMinion,
    Taunt,
    DialogueStep
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