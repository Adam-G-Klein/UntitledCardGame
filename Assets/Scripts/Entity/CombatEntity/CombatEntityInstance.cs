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
        applyStatusEffects(info.statusEffects);
        takeDamage(info.damage);
    }
    protected void applyStatusEffect(StatusEffect effect, int scale){
        switch(effect) {
            case(StatusEffect.Weakness):
                stats.statusEffects[StatusEffect.Weakness] += scale;
                break;
            case(StatusEffect.Strength):
                Debug.Log("Applying strength effect to " + this.id);
                stats.statusEffects[StatusEffect.Strength] += scale;
                break;
            case(StatusEffect.Defended):
                stats.statusEffects[StatusEffect.Defended] += scale;
                break;
        }
    }
    protected void applyStatusEffects(Dictionary<StatusEffect, int> effects){
        Debug.Log("Applying status effects for " + this.id);
        foreach(KeyValuePair<StatusEffect, int> effect in effects){
            applyStatusEffect(effect.Key, effect.Value);
        }
    }

    protected void takeDamage(int damage){
        stats.currentHealth = Mathf.Max(stats.currentHealth - damageAfterDefense(damage), 0);
        if(stats.currentHealth == 0){
            StartCoroutine(onDeath());
        }
    }

    protected int damageAfterDefense(int damage){
        if(!stats.statusEffects.ContainsKey(StatusEffect.Defended)) 
            return damage;
        stats.statusEffects[StatusEffect.Defended] -= damage;
        if(stats.statusEffects[StatusEffect.Defended] < 0){
            damage = -stats.statusEffects[StatusEffect.Defended];
            stats.statusEffects[StatusEffect.Defended] = 0;
        } else {
            damage = 0;
        }
        return damage;
    }
}
