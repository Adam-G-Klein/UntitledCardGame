using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionAbility
{
    [SerializeField]
    public CompanionAbilityTrigger companionAbilityTrigger;
    [SerializeReference]
    public List<EffectStep> effectSteps;
    public EffectWorkflow effectWorkflow { get {
        return new EffectWorkflow(effectSteps);
        }
    }

    public enum CompanionAbilityTrigger {
        EnterTheBattlefield,
        EndOfCombat,
        EndOfPlayerTurn,
        EndOfEnemyTurn,
        OnFriendOrFoeDeath,
        OnFriendDeath,
        OnDeath,
        OnCardCast,
        OnCombine,
        OnCardExhausted,
        OnDeckShuffled,
        OnFriendDamageTaken,
        StartOfPlayerTurn,
    }
}