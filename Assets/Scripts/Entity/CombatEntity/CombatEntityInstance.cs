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
    [SerializeField]
    protected TurnPhaseTriggerEvent registerTurnPhaseTriggerEvent;
    [SerializeField]
    protected TurnPhaseTriggerEvent removeTurnPhaseTriggerEvent;
    protected TurnManager turnManager;
    // keeping a reference to the trigger so we can remove it on death
    private TurnPhaseTrigger updateStatusTrigger;

    protected override void Start() {
        base.Start();
        this.stats = new CombatEntityInEncounterStats(baseStats);
        // StartCoroutine(instantiatedEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityInstantiatedEventInfo(this)));
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null) {
            turnManager = turnManagerObject.GetComponent<TurnManager>();
        } else {
            Debug.LogError("TurnManager not found, won't be able to do one turn effects");
        }
        registerUpdateStatusEffects();
    }

    protected virtual IEnumerator onDeath(CombatEntityInstance killer) {
        Debug.Log("OnDeath called for " + this.id + " with killer " + killer?.id);
        processOnDeathStatusEffects(killer);
        yield return StartCoroutine(removeTurnPhaseTriggerEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseTriggerEventInfo(updateStatusTrigger)));
        CombatEntityManager.Instance.combatEntityDied(this);
        Destroy(this.gameObject);
    }

    // tbh this method could be good reason to rethink how we do status effects.
    // like, should they be able to call their own custom coroutines when something
    // happens to the entity they're on?
    private void processOnDeathStatusEffects(CombatEntityInstance killer) {
        if(killer != null && stats.statusEffects[StatusEffect.MaxHpBounty] > 0) {
            Debug.Log("Adding bounty to " + killer.id);
            // gotta make this an event somehow, don't think the mechanics team
            // knows how important it'll be to hang onto though
            killer.baseStats.setMaxHealth(killer.baseStats.getMaxHealth() + stats.statusEffects[StatusEffect.MaxHpBounty]);
        }
        // Okay, checked with the mechanics team and we do not need this right now
        // if we do need to do this someday, it's somewhat complicated.
        // Best idea I've had was to have special event for it, just have the 
        // listener for it on the companion instances, and create a new method on BaseGameEvent
        // for calling just an int subset of an event's listeners. Felt like way too much 
        // pattern setting just for one status effect though, so letting this chill
        // until we need to do something similar
        /*
        if(stats.statusEffects[StatusEffect.MinionsOnDeath] > 0) {
            spawnMinions(stats.statusEffects[StatusEffect.MinionsOnDeath]);
        }
        */
    }

    public void combatEffectEventHandler(CombatEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        applyCombatEffects(info.combatEffects, info.effector);
    }

    public void applyStatusEffects(StatusEffect statusEffect, int scale) {
        stats.statusEffects[statusEffect] += scale;
    }

    protected void applyCombatEffects(Dictionary<CombatEffect, int> effects, CombatEntityInstance effector){
        Debug.Log("Applying combat effects for " + this.id);
        CombatEffectEvent.applyCombatEffectStatuses(effects, stats.statusEffects);
        foreach(KeyValuePair<CombatEffect, int> effect in effects){
            applyNonStatusCombatEffect(effect.Key, effect.Value, effector);
        }
    }
    
    public void applyNonStatusCombatEffect(CombatEffect effect, int scale, CombatEntityInstance effector) {
        // All the non-status-effect combat effects are handled here
        // status effects are handled in applyCombatEffects
        switch(effect) {
            case(CombatEffect.Damage):
                takeDamage(scale, effector);
                break;
            case(CombatEffect.Heal):
                stats.currentHealth = Mathf.Min(stats.currentHealth + scale, stats.maxHealth);
                break;
            case(CombatEffect.DrawFrom):
                onDraw(scale); //method overridden by CombatEntityWithDeckInstance
                break;
            case(CombatEffect.SetHealth):
                // won't work for setting health to 0, misses the death check. Pretty sure we should just use sacrifice for that
                // in all cases though
                stats.currentHealth = scale;
                break;
            case(CombatEffect.Sacrifice):
                StartCoroutine(onDeath(effector));
                break;
        }
    }
    

    protected void takeDamage(int damage, CombatEntityInstance attacker){
        stats.currentHealth = Mathf.Max(stats.currentHealth - damageAfterDefense(damage), 0);
        if(stats.currentHealth == 0){
            StartCoroutine(onDeath(attacker));
        }
    }

    protected int damageAfterDefense(int damage) {
        if(stats.statusEffects[StatusEffect.Invulnerability] > 0)
            return 0;
        if(stats.statusEffects[StatusEffect.PlatedArmor] > damage) {
            damage = 0;
        }
        if(stats.statusEffects[StatusEffect.Defended] == 0) 
            return damage;
        stats.statusEffects[StatusEffect.Defended] -= damage;
        if(stats.statusEffects[StatusEffect.Defended] < 0){
            damage = -stats.statusEffects[StatusEffect.Defended];
            stats.statusEffects[StatusEffect.Defended] = 0;
        } else {
            damage = 0;
        }
        if(damage > 0) {
            stats.statusEffects[StatusEffect.PlatedArmor] -= 1;
        }
        return damage;
    }

    // overridden by CombatEntityWithDeckInstance
    protected virtual void onDraw(int scale) {}

    // For effects that need to be updated every turn
    // right now we just have this run on the end of the player turn for companions
    // and the end of enemy turn for enemies, but we can set up a more complex 
    // mapping if we want some effects to update at different times

    private void registerUpdateStatusEffects(){
        TurnPhase updatePhase = entityType == EntityType.Enemy ? TurnPhase.END_ENEMY_TURN : TurnPhase.END_PLAYER_TURN;
        updateStatusTrigger = new TurnPhaseTrigger(updatePhase, updateStatusEffects());
        StartCoroutine(registerTurnPhaseTriggerEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseTriggerEventInfo(updateStatusTrigger)));
    }

    private IEnumerable updateStatusEffects() {
        stats.statusEffects[StatusEffect.Defended] = 0;
        stats.statusEffects[StatusEffect.Invulnerability] = Mathf.Max(0, stats.statusEffects[StatusEffect.Invulnerability] - 1);
        stats.statusEffects[StatusEffect.TemporaryStrength] = 0;
        yield return null;
    }



}
