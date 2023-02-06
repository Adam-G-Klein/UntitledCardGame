using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatEffectEventListener))]
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

    public void combatEffectEventHandler(CombatEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        applyCombatEffects(info.combatEffects);
    }
    protected void applyNonStatusCombatEffect(CombatEffect effect, int scale){
        // All the non-status-effect combat effects are handled here
        // status effects are handled in applyCombatEffects
        switch(effect) {
            case(CombatEffect.Damage):
                takeDamage(scale);
                break;
            case(CombatEffect.DrawFrom):
                onDraw(scale); //overridden by CombatEntityWithDeckInstance
                break;
            case(CombatEffect.SetHealth):
                stats.currentHealth = scale;
                break;
        }
    }
    protected void applyCombatEffects(Dictionary<CombatEffect, int> effects){
        Debug.Log("Applying combat effects for " + this.id);
        CombatEffectEvent.applyCombatEffectStatuses(effects, stats.statusEffects);
        foreach(KeyValuePair<CombatEffect, int> effect in effects){
            applyNonStatusCombatEffect(effect.Key, effect.Value);
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

    //overridden by CombatEntityWithDeckInstance
    protected virtual void onDraw(int scale) {}
}
