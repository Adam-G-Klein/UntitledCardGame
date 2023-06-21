using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInstance : CombatEntityWithDeckInstance 
{
    public Companion companion;

    protected override void Start() {
        base.Start();
        CombatEntityManager.Instance.registerCompanion(this);
    }

    protected override IEnumerator onDeath(CombatEntityInstance killer)
    {
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

