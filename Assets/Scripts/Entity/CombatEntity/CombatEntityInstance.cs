using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyEffectEventListener))]
// Extended by both companionInstance and enemyInstance
public abstract class CombatEntityInstance: TargettableEntity
{
    
    // set in the prefab instantiator
    public CombatEntityBaseStats baseStats;
    public CombatEntityInEncounterStats stats;
    [SerializeField]
    private CombatEntityInstantiatedEvent instantiatedEvent;
    [SerializeField]
    protected CombatEntityDeathEvent deathEvent;

    protected virtual void Start() {
        this.stats = new CombatEntityInEncounterStats(baseStats);
        StartCoroutine(instantiatedEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityInstantiatedEventInfo(this)));
    }

    protected virtual IEnumerator onDeath() {
        Debug.Log("Entity " + this.id + " is dying");
        yield return StartCoroutine(deathEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityDeathEventInfo(this)));
        // TODO, probably need to improve this
        Destroy(this.gameObject);
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
            StartCoroutine(onDeath());
        }
    }
    protected void applyStatusEffect(StatusEffect effect, int scale){
        switch(effect) {
            case(StatusEffect.Weakness):
                stats.statusEffects[StatusEffect.Weakness] += scale;
                break;
            case(StatusEffect.Strength):
                stats.statusEffects[StatusEffect.Strength] += scale;
                break;
        }
    }
}
