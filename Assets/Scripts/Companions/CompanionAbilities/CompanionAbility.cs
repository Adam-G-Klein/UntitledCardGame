using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionAbilityContext {

    public CompanionAbilityInvoker invoker;
    public TurnManager turnPhaseManager;
    public PlayerHand playerHand;
    public CompanionManager companionManager;
    public CompanionInstance companionInstance;

    public CompanionAbilityContext(CompanionAbilityInvoker invoker, TurnManager turnPhaseManager, PlayerHand playerHand, CompanionManager companionManager, CompanionInstance companionInstance)
    {
        this.invoker = invoker;
        this.turnPhaseManager = turnPhaseManager;
        this.playerHand = playerHand;
        this.companionManager = companionManager;
        this.companionInstance = companionInstance;
    }
    
}

[System.Serializable]
public abstract class CompanionAbility : TargetRequester
{

    public string abilityName;
    public abstract void setupAbility(CompanionAbilityContext context);

    // Using IEnumerable so that we can restart the method from the list of turnphase triggers each time
    // apparently IEnumerator can't be restarted: https://forum.unity.com/threads/i-cant-call-a-coroutine-from-a-list-for-second-time.999421/
    public abstract IEnumerable invoke(CompanionAbilityContext context);

    public abstract void onDeath(CompanionAbilityContext context);

    public virtual void resetAbilityState()
    {
        base.resetTargets();
    }
}
