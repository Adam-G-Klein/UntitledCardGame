using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionAbilityContext {

    public CompanionAbilityInvoker invoker;
    public TurnManager turnPhaseManager;
    public PlayerHand playerHand;

    public CompanionAbilityContext(CompanionAbilityInvoker invoker, TurnManager turnPhaseManager, PlayerHand playerHand)
    {
        this.invoker = invoker;
        this.turnPhaseManager = turnPhaseManager;
        this.playerHand = playerHand;
    }
}
[System.Serializable]
public abstract class CompanionAbility 
{

    public string abilityName;
    protected List<TargettableEntity> currentAbilityTargets;
    public abstract void setupAbility(CompanionAbilityContext context);
    public virtual void resetAbilityState() {
        currentAbilityTargets.Clear();
    }

    public void targetsSupplied(List<TargettableEntity> targets)
    {
        this.currentAbilityTargets = targets;
    }

    public abstract IEnumerator invoke(CompanionAbilityContext context);
}
