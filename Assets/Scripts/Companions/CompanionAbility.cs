using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionAbility
{
    [SerializeField]
    private CompanionAbilityTrigger companionAbilityTrigger;
    [SerializeReference]
    public List<EffectStep> effectSteps;
}

public enum CompanionAbilityTrigger {
    EnterTheBattlefield,
    EndOfCombat,
    EndOfPlayerTurn,
    OnFriendOrFoeDeath,
    OnDeath
}