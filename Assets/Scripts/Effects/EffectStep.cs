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
    CombatInstanceCacheLoad,
    CombatInstanceCacheStore,
    ConvertPlayableCardToCard,
    ConvertPlayableCardToDeckInstance,
    CountCardsInDeck,
    CountStatusEffectsStep,
    CountStep,
    DebugEffectStep,
    Default,
    DrawCards,
    EndWorkflowIfConditionMet,
    EndWorkflowIfListEmpty,
    EndWorkflowIfNoMapElement,
    FilterCards,
    FilterEntityByCardsInDeck,
    FilterEntityByHP,
    FilterEntityByStatus,
    GetAdjacentCompanions,
    GetCardsFromDeck,
    GetCardsFromDiscard,
    GetCastCountFromCard,
    GetDecksShuffledThisCombat,
    GetHPStatistics,
    GetNumberOfCardsInHand,
    GetRandomItems,
    GetTargets,
    GoldManipulation,
    HasCardActionBeenTakenThisTurn,
    InstantiatePrefab,
    ManaChange,
    ManualOnCombineInShop,
    MathStep,
    PermanentStatIncrease,
    QuantityOfCardTypePlayedThisTurn,
    RemoveStatus,
    SelectCardsFromList,
    SetCardEffectWorkflow,
    SummonMinion,
    Taunt,
    TransmogrifyCard,
    DialogueStep,
    FXEffectStep,
    WaitForSecondsEffect,
    ScriptableObjectEffectStep,
    EntityAbilityTriggeredVFX
}

[System.Serializable]
public abstract class EffectStep
{
    [HideInInspector]
    public string effectStepName;
    public virtual IEnumerator invoke(EffectDocument document) {
        yield return null;
        Debug.Log("EffectStep: UpdateView");
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    protected void EffectError(string errorString) {
        Debug.LogError(effectStepName + " Effect: " + errorString);
    }

    protected void EffectLog(string logString) {
        Debug.Log("[" + effectStepName + "]" + logString);
    }
}