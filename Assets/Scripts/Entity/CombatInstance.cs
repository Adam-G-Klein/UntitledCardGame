using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CombatInstance : MonoBehaviour
{
    public CombatStats combatStats;

    public delegate IEnumerator OnDeathHandler(CombatInstance killer);
    public event OnDeathHandler onDeathHandler;

    // Only used for debugging purposes, will be set by some other script
    private string id;

    public static Dictionary<StatusEffect, int> initialStatusEffects = 
        new Dictionary<StatusEffect, int>() {
            { StatusEffect.Strength, 0 },
            { StatusEffect.Weakness, 0 },
            { StatusEffect.Defended, 0 },
            { StatusEffect.DamageMultiply, 1 },
            { StatusEffect.Invulnerability, 0 },
            { StatusEffect.MaxHpBounty, 0 },
            { StatusEffect.TemporaryStrength, 0 },
            { StatusEffect.MinionsOnDeath, 0 },
            { StatusEffect.PlatedArmor, 0 }
        };

    public Dictionary<StatusEffect, int> statusEffects = 
        new Dictionary<StatusEffect, int> (initialStatusEffects);

    public void Start() {

    }

    public void ApplyStatusEffects(StatusEffect statusEffect, int scale) {
        statusEffects[statusEffect] += scale;
    }

    public int GetCurrentDamage() {
        return Mathf.Max(0, (
                combatStats.baseAttackDamage 
                + statusEffects[StatusEffect.Strength]
                + statusEffects[StatusEffect.TemporaryStrength]
                - statusEffects[StatusEffect.Weakness]));
    }

    public void ApplyNonStatusCombatEffect(CombatEffect effect, int scale, CombatInstance effector) {
        // All the non-status-effect combat effects are handled here
        // status effects are handled in applyCombatEffects
        switch(effect) {
            case CombatEffect.Damage:
            case CombatEffect.FixedDamage:
                TakeDamage(scale, effector);
                break;
            case CombatEffect.Heal:
                combatStats.currentHealth = Mathf.Min(combatStats.currentHealth + scale, combatStats.maxHealth);
                break;
            case CombatEffect.DrawFrom:
                // This no longer needs to be here, completely handled by DeckInstance
                break;
            case CombatEffect.SetHealth:
                // won't work for setting health to 0, misses the death check. Pretty sure we should just use sacrifice for that
                // in all cases though
                combatStats.currentHealth = scale;
                break;
            case CombatEffect.Sacrifice:
                StartCoroutine(OnDeath(effector));
                break;
        }
    }

    private void TakeDamage(int damage, CombatInstance attacker){
        combatStats.currentHealth = Mathf.Max(combatStats.currentHealth - DamageAfterDefense(damage), 0);
        if(combatStats.currentHealth == 0){
            StartCoroutine(OnDeath(attacker));
        }
    }

    private int DamageAfterDefense(int damage) {
        // Invulnerability removal is handled at end of turn
        if (statusEffects[StatusEffect.Invulnerability] > 0)
            return 0;
        
        if (statusEffects[StatusEffect.PlatedArmor] > 0) {
            // We have plated armor, so set damage and remove 1 armor
            damage = 0;
            statusEffects[StatusEffect.PlatedArmor] -= 1;
        }

        // No block, so just taking full damage
        if(statusEffects[StatusEffect.Defended] == 0) 
            return damage;
        
        if (statusEffects[StatusEffect.Defended] > damage) {
            statusEffects[StatusEffect.Defended] -= damage;
            damage = 0;
        } else if (statusEffects[StatusEffect.Defended] < damage) {
            damage -= statusEffects[StatusEffect.Defended];
            statusEffects[StatusEffect.Defended] = 0;
        } else {
            statusEffects[StatusEffect.Defended] = 0;
            damage = 0;
        }

        return damage;
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        string blockerId = Id.newGuid();
        TurnManager.Instance.addTurnPhaseBlocker(blockerId);
        Debug.Log("OnDeath called for " + this.id + " with killer " + killer.GetId());
        ProcessOnDeathStatusEffects(killer);
        if (onDeathHandler != null) {
            foreach (OnDeathHandler handler in onDeathHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke(killer));
            }
        }
        // CombatEntityManager.Instance.combatEntityDied(this);
        TurnManager.Instance.removeTurnPhaseBlocker(blockerId);
        Destroy(this.gameObject);
        yield return null;
    }

    // tbh this method could be good reason to rethink how we do status effects.
    // like, should they be able to call their own custom coroutines when something
    // happens to the entity they're on?
    private void ProcessOnDeathStatusEffects(CombatInstance killer) {
        if (killer != null && statusEffects[StatusEffect.MaxHpBounty] > 0) {
            killer.combatStats.maxHealth += statusEffects[StatusEffect.MaxHpBounty];
            killer.combatStats.currentHealth += statusEffects[StatusEffect.MaxHpBounty];
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

    public IEnumerable UpdateStatusEffects() {
        statusEffects[StatusEffect.Defended] = 0;
        statusEffects[StatusEffect.TemporaryStrength] = 0;
        statusEffects[StatusEffect.Invulnerability] = Mathf.Max(0, statusEffects[StatusEffect.Invulnerability] - 1);
        statusEffects[StatusEffect.Weakness] = Mathf.Max(0, statusEffects[StatusEffect.Weakness] - 1);
        yield return null;
    }

    // This function is setup the way it is because certain statuses need to be
    // updated at different times. Making this function take in a list of statuses
    // allows us to separate when one status is updated vs another
    public IEnumerable UpdateStatusEffects(List<StatusEffect> statuses) {
        foreach (StatusEffect effect in statuses) {
            UpdateStatusEffect(effect);
        }
        yield return null;
    }

    private void UpdateStatusEffect(StatusEffect status) {
        switch (status) {
            case StatusEffect.Defended:
                statusEffects[StatusEffect.Defended] = 0;
            break;

            case StatusEffect.TemporaryStrength:
                statusEffects[StatusEffect.TemporaryStrength] = 0;
            break;

            case StatusEffect.Invulnerability:
                statusEffects[StatusEffect.Invulnerability] = Mathf.Max(0, statusEffects[StatusEffect.Invulnerability] - 1);
            break;

            case StatusEffect.Weakness:
                statusEffects[StatusEffect.Weakness] = Mathf.Max(0, statusEffects[StatusEffect.Weakness] - 1);
            break;
        }
    }


    public void SetId(string id) {
        this.id = id;
    }

    public string GetId() {
        return this.id;
    }
}