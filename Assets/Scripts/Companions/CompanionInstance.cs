using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompanionAbilityInvoker))]
public class CompanionInstance : CombatEntityFriendly 
{
    public Companion companion;
    private CompanionAbilityInvoker abilityInvoker;

    protected override void Start() {
        base.Start();
        abilityInvoker = GetComponent<CompanionAbilityInvoker>();
    }

    protected override IEnumerator onDeath(CombatEntityInstance killer)
    {
        abilityInvoker.onDeath();
        return base.onDeath(killer);
    }

    public override bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo)
    {
        // TODO: figure out a way to prevent companions from drawing from an empty deck
        // like so but with a check before to see if it's a draw effect:
        /*
        return inCombatDeck.drawPile.Count > 0 
            || inCombatDeck.discardPile.Count > 0;
            */
        return true;
    }

}

