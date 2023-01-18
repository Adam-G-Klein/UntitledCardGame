using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extended by both companionInstance and enemyInstance
public abstract class CombatEntityInstance: TargettableEntity
{
    
    public CombatEntityBaseStats baseStats;
    public CombatEntityInEncounterStats stats;
    public abstract CombatEntityInEncounterStats getCombatEntityInEncounterStats();
    [SerializeField]
    private CombatEntityInstantiatedEvent instantiatedEvent;
    [SerializeField]
    protected CombatEntityDeathEvent deathEvent;

    protected virtual void Start() {
        StartCoroutine(instantiatedEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityInstantiatedEventInfo(this)));
    }

    // This could totally work for companion effects too, just need to 
    // abstract the info passed in to allow for card draw
    // Also unsure if that should be done at all 
    public void enemyEffectEventHandler(EnemyEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        if(this is EnemyInstance){
            Debug.Log("Enemy " + this.id + " is getting buffer");
        }
        foreach(KeyValuePair<StatusEffect, int> effect in info.statusEffects){
            applyStatusEffect(effect.Key, effect.Value);
        }
        stats.currentHealth = Mathf.Max(stats.currentHealth - info.damage, 0);
        if(stats.currentHealth == 0){
            StartCoroutine(deathEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityDeathEventInfo(this)));
        }
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
