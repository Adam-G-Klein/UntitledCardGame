using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companionInstance and enemyInstance
public abstract class CombatEntityInstance: MonoBehaviour
{
    
    public CombatEntityBaseStats baseStats;
    public CombatEntityInEncounterStats stats;
    public abstract CombatEntityInEncounterStats getCombatEntityInEncounterStats();
    
    // This could totally work for companion effects too, just need to 
    // abstract the info passed in to allow for card draw
    // Also unsure if that should be done at all 
    public void enemyEffectEventHandler(EnemyEffectEventInfo info){
        if(!info.targets.Contains(this.baseStats.getId())) return;
        Debug.Log("Companion " + this.baseStats.getId() + " is being affected by " + info.ToString());
        foreach(KeyValuePair<StatusEffect, int> effect in info.statusEffects){
            applyStatusEffect(effect.Key, effect.Value);
        }
        stats.currentHealth -= info.damage;
    }
    protected void applyStatusEffect(StatusEffect effect, int scale){
        switch(effect) {
            case(StatusEffect.Weakness):
                stats.weakness += scale;
                break;
            case(StatusEffect.Strength):
                stats.strength += scale;
                break;
        }
    }
}
