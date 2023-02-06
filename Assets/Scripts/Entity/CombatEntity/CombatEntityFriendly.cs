using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just in the hierarchy so that we don't duplicate this code for minions
// and companions 
public abstract class CombatEntityFriendly : CombatEntityWithDeckInstance
{
    private TurnPhaseTrigger clearBlockTrigger;
    protected override void Start() {
        base.Start();
        clearBlockTrigger = new TurnPhaseTrigger(TurnPhase.END_ENEMY_TURN, clearBlock());
        turnManager.addTurnPhaseTrigger(clearBlockTrigger);
    }

    private IEnumerable clearBlock() {
        stats.statusEffects[StatusEffect.Defended] = 0;
        yield return null;
    }

    protected override IEnumerator onDeath() {
        turnManager.removeTurnPhaseTrigger(clearBlockTrigger);
        yield return base.onDeath();
    }
    
    
}

