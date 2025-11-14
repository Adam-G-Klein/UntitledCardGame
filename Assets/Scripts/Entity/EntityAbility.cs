using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAbility
{
    [SerializeField]
    public EntityAbilityTrigger abilityTrigger;
    [SerializeReference]
    public List<EffectStep> effectSteps;
    public EffectWorkflow effectWorkflow
    {
        get
        {
            return new EffectWorkflow(effectSteps);
        }
    }
    // For a given EntityAbilityTrigger, weight sorts the abilities so we get deterministic ordering of effects.
    public int weight = 0;

    public enum EntityAbilityTrigger
    {
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
        OnEntityDamageTaken,
        StartOfPlayerTurn,
        OnHandEmpty,
        OnFoeDeath,
        OnEntityHealed,
        OnCardDiscard,
        OnBlockGained,
        OnAttackCardCast,
        OnCardDraw,
    }
}
