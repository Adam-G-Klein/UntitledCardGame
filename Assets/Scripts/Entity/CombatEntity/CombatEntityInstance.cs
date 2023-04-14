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

    protected override void Start() {
        base.Start();
        this.stats = new CombatEntityInEncounterStats(baseStats);
        StartCoroutine(instantiatedEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityInstantiatedEventInfo(this)));
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null) {
            turnManager = turnManagerObject.GetComponent<TurnManager>();
        } else {
            Debug.LogError("TurnManager not found, won't be able to do one turn effects");
        }
    }

    protected virtual IEnumerator onDeath(CombatEntityInstance killer) {
        Debug.Log("OnDeath called for " + this.id + " with killer " + killer?.id);
        // can replace this with a process bounties function if we get more of them
        if(killer != null && stats.statusEffects[StatusEffect.MaxHpBounty] > 0) {
            Debug.Log("Adding bounty to " + killer.id);
            killer.baseStats.setMaxHealth(killer.baseStats.getMaxHealth() + stats.statusEffects[StatusEffect.MaxHpBounty]);
        }
        yield return StartCoroutine(deathEvent.RaiseAtEndOfFrameCoroutine(new CombatEntityDeathEventInfo(this)));
        // TODO, probably need to improve this
        Destroy(this.gameObject);
    }

    public void combatEffectEventHandler(CombatEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        applyCombatEffects(info.combatEffects, info.effector);
    }
    protected void applyCombatEffects(Dictionary<CombatEffect, int> effects, CombatEntityInstance effector){
        Debug.Log("Applying combat effects for " + this.id);
        CombatEffectEvent.applyCombatEffectStatuses(effects, stats.statusEffects);
        foreach(KeyValuePair<CombatEffect, int> effect in effects){
            applyNonStatusCombatEffect(effect.Key, effect.Value, effector);
        }
    }
    
    protected void applyNonStatusCombatEffect(CombatEffect effect, int scale, CombatEntityInstance effector){
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
                onDraw(scale); //overridden by CombatEntityWithDeckInstance
                break;
            case(CombatEffect.SetHealth):
                // won't work for setting health to 0. Pretty sure we should just use sacrifice for that
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

    protected int damageAfterDefense(int damage){
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

    //overridden by CombatEntityWithDeckInstance
    protected virtual void onDraw(int scale) {}

    /* We'll do this if we need to do one turn effects for more than block and strength
    public void oneTurnEffect(CombatEffect effect, int scale){
        CombatEffectEvent.applyCombatEffectStatuses(new Dictionary<CombatEffect, int>(){ {effect, scale} }, stats.statusEffects);
        TurnPhase endPhase;
        if(this is EnemyInstance) {
            endPhase = TurnPhase.END_ENEMY_TURN;
        } else {
            endPhase = TurnPhase.END_PLAYER_TURN;
        }
        oneTurnEffectTrigger = new TurnPhaseTrigger(endPhase, removeStatusEffect(CombatEffectEvent.combatEffectToStatusEffect[effect], scale));
        turnManager.addTurnPhaseTrigger(oneTurnEffectTrigger);
    }

    private IEnumerable removeStatusEffect(StatusEffect effect, int scale){
        stats.statusEffects[effect] -= scale;
        turnManager.removeTurnPhaseTrigger(oneTurnEffectTrigger);
        oneTurnEffectTrigger = null;
        yield return null;
    }
    */

}
