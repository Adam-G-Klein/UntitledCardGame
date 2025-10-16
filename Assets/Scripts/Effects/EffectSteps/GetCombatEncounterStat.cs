using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCombatEncounterStat : EffectStep, IEffectStepCalculation
{
    public enum StatType
    {
        NumCardsDiscardedThisTurn,
        NumCardsPlayedThisTurn,
    }

    [SerializeField]
    private StatType statOfInterest;

    [SerializeField]
    private string outputKey = "";

    public GetCombatEncounterStat()
    {
        effectStepName = "GetCombatEncounterStat";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        switch (statOfInterest)
        {
            case StatType.NumCardsDiscardedThisTurn:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsDiscardedThisTurn.Count;
                break;
            case StatType.NumCardsPlayedThisTurn:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisTurn.Count;
                break;
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
