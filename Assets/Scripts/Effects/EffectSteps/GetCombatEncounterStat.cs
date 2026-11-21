using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetCombatEncounterStat : EffectStep, IEffectStepCalculation
{
    public enum StatType
    {
        NumCardsDiscardedThisTurn,
        NumCardsPlayedThisTurn,
        NumCardsExhaustedThisTurn,
        NumCardsExhaustedThisCombat,
        NumExtraCardsDrawnThisTurn,
        NumCharmsExhaustedThisCombat,
        NumCardsPlayedThisCombat,
        NumElanSpentThisTurn,
        NumAttacksPlayedThisCombat,
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
            case StatType.NumCardsExhaustedThisTurn:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsExhaustThisTurn.Count;
                break;
            case StatType.NumCardsExhaustedThisCombat:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsExhaustThisCombat.Count;
                break;
            case StatType.NumExtraCardsDrawnThisTurn:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.GetNumExtraCardsDrawnThisTurn();
                break;
            case StatType.NumCharmsExhaustedThisCombat:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsExhaustThisCombat
                    .Count(card => card.cardType.cardCategory == CardCategory.Charm);
                break;
            case StatType.NumCardsPlayedThisCombat:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisCombat.Count;
                break;
            case StatType.NumElanSpentThisTurn:
                document.intMap[outputKey] = ManaManager.Instance.manaSpentThisTurn;
                break;
            case StatType.NumAttacksPlayedThisCombat:
                document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisCombat
                    .Count(card => card.cardType.cardCategory == CardCategory.Attack);
                break;
            default:
                Debug.LogError("Unhandled stat type: " + statOfInterest);
                break;
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
