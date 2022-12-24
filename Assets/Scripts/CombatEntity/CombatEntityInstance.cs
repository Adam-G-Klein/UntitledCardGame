using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extended by both companionInstance and enemyInstance
public abstract class CombatEntityInstance: TargettableEntity
{
    
    public CombatEntityBaseStats baseStats;
    public CombatEntityInEncounterStats stats;
    public abstract CombatEntityInEncounterStats getCombatEntityInEncounterStats();

    // This could totally work for companion effects too, just need to 
    // abstract the info passed in to allow for card draw
    // Also unsure if that should be done at all 
    public void enemyEffectEventHandler(EnemyEffectEventInfo info){
        if(!info.targets.Contains(this.id)) return;
        Debug.Log("Companion " + this.id + " is being affected by " + info.ToString());
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
