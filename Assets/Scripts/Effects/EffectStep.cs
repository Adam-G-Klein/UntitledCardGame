using System.Collections;
using UnityEngine;

public enum EffectStepName
{
    ActivatePower,
    AddCardsToDeck,
    AddCardsToHand,
    AddManaNextTurn,
    ApplyStatus,
    BooleanComparison,
    CardInDeckEffect,
    CardInHandEffect,
    CardModificationEffect,
    ChangeEnemyIntent,
    CheckTypeOfMostRecentlyPlayedCard,
    CheckCurrentTurnPhase,
    CombatEffectStep,
    CombatInstanceCacheLoad,
    CombatInstanceCacheStore,
    CombatInstanceTookDamageThisTurn,
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
    GetCombatEncounterStat,
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
    SpriteSwap,
    SummonMinion,
    Taunt,
    TransmogrifyCard,
    DialogueStep,
    FXEffectStep,
    WaitForSecondsEffect,
    ScriptableObjectEffectStep,
    EntityAbilityTriggeredVFX,
    CardTriggeredVFX
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

    // Whether this step plays a sound of its own when run for this card (status sounds, heals, attack sounds).
    // CardStepSfx uses it to give otherwise-silent card workflows a sound without doubling up on loud ones.
    public virtual bool PlaysSoundFor(CardType card) {
        return false;
    }

    protected void EffectError(string errorString) {
        Debug.LogError(effectStepName + " Effect: " + errorString);
    }

    protected void EffectLog(string logString) {
        Debug.Log("[" + effectStepName + "]" + logString);
    }
}