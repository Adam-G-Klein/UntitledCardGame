using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just in the hierarchy so that we don't duplicate this code for minions
// and companions 
public abstract class CombatEntityFriendly : CombatEntityWithDeckInstance
{
    private TurnPhaseTrigger updateStatusTrigger;
    protected override void Start() {
        base.Start();
        updateStatusTrigger = new TurnPhaseTrigger(TurnPhase.END_ENEMY_TURN, updateStatus());
        turnManager.addTurnPhaseTrigger(updateStatusTrigger);
    }

    // should just move this whole class into CombatEntityInstance and check if it's an enemy or not for the turn phase
    private IEnumerable updateStatus() {
        stats.statusEffects[StatusEffect.Defended] = 0;
        stats.statusEffects[StatusEffect.Invulnerability] = Mathf.Max(0, stats.statusEffects[StatusEffect.Invulnerability] - 1);
        stats.statusEffects[StatusEffect.TemporaryStrength] = 0;
        // this should clear at the end of the player's turn
        //stats.statusEffects[StatusEffect.Weakness] = Mathf.Max(0, stats.statusEffects[StatusEffect.Weakness] - 1);
        yield return null;
    }

    protected override IEnumerator onDeath(CombatEntityInstance killer) {
        turnManager.removeTurnPhaseTrigger(updateStatusTrigger);
        yield return base.onDeath(killer);
    }
    
    
}

