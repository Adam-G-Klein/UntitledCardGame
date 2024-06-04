using System;
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

    public AudioClip genericInteractionSFX;
    public GameObject genericInteractionVFX;

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
            { StatusEffect.PlatedArmor, 0 },
            { StatusEffect.Orb, 0 },
            { StatusEffect.Thorns, 0 },
            { StatusEffect.MoneyOnDeath, 0 }
        };

    public Dictionary<StatusEffect, int> statusEffects = 
        new Dictionary<StatusEffect, int> (initialStatusEffects);

    public void Start() {

    }

    public void ApplyStatusEffects(StatusEffect statusEffect, int scale) {
        Debug.Log(String.Format("Applying status with scale {0}", scale));
        statusEffects[statusEffect] += scale;
        if(statusEffect != StatusEffect.Orb && statusEffect != StatusEffect.Strength && statusEffect != StatusEffect.TemporaryStrength) {
            PlaySFX();
            AddVFX();
        }
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
                PlaySFX(effector);
                AddVFX(effector);
                break;
            case CombatEffect.Heal:
                combatStats.setCurrentHealth(Mathf.Min(combatStats.getCurrentHealth() + scale, combatStats.maxHealth));
                break;
            case CombatEffect.DrawFrom:
                // This no longer needs to be here, completely handled by DeckInstance
                break;
            case CombatEffect.SetHealth:
                // won't work for setting health to 0, misses the death check. Pretty sure we should just use sacrifice for that
                // in all cases though
                combatStats.setCurrentHealth(scale);
                break;
            case CombatEffect.Sacrifice:
                StartCoroutine(OnDeath(effector));
                break;
        }
    }

    private void TakeDamage(int damage, CombatInstance attacker) {
        
        // This is necessary to solve a race condition with a multi-damage attack
        // Fix this later
        if (combatStats.getCurrentHealth() == 0) {
            return;
        }
        Debug.Log("Take Damage is Setting current health to " + Mathf.Max(combatStats.getCurrentHealth() - DamageAfterDefense(damage), 0));
        combatStats.setCurrentHealth(Mathf.Max(combatStats.getCurrentHealth() - DamageAfterDefense(damage), 0));

        if (combatStats.getCurrentHealth() == 0){
            StartCoroutine(OnDeath(attacker));
        }

        if (statusEffects[StatusEffect.Thorns] > 0) {
            attacker.ApplyNonStatusCombatEffect(CombatEffect.FixedDamage, statusEffects[StatusEffect.Thorns], this);
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
            killer.combatStats.setCurrentHealth(killer.combatStats.getCurrentHealth() + statusEffects[StatusEffect.MaxHpBounty]);
        }

        if (statusEffects[StatusEffect.MoneyOnDeath] > 0) {
            PlayerData playerData = EnemyEncounterManager.Instance.gameState.playerData.GetValue();
            playerData.gold += statusEffects[StatusEffect.MoneyOnDeath];
        }
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
                statusEffects[status] = 0;
                break;
            case StatusEffect.Thorns:
                statusEffects[status] = 0;
                break;

            case StatusEffect.TemporaryStrength:
                statusEffects[status] = 0;
                break;
            case StatusEffect.Invulnerability:
                statusEffects[status] = 0;
                break;
            case StatusEffect.Weakness:
                statusEffects[status] = DecrementStatus(statusEffects[status]);
                break;
            case StatusEffect.Orb:
                break;
            
            // This is separate from the above for now since this might
            // need special logic
            case StatusEffect.MoneyOnDeath:
                statusEffects[status] = DecrementStatus(statusEffects[status]);
            break;
        }
    }

    private int DecrementStatus(int currentCount) {
        return Mathf.Max(0, currentCount - 1);
    }


    public void SetId(string id) {
        this.id = id;
    }

    public string GetId() {
        return this.id;
    }

    private void PlaySFX(CombatInstance effector = null) {
        if(genericInteractionSFX != null) {
            MusicController.Instance.PlaySFX(genericInteractionSFX);
        } else if (effector != null && effector.genericInteractionSFX != null) {
            MusicController.Instance.PlaySFX(effector.genericInteractionSFX);
        }
    }

    private void AddVFX(CombatInstance effector = null) {
        if(effector != null && effector.genericInteractionSFX != null) {
            Instantiate(effector.genericInteractionVFX, transform.position, Quaternion.identity);
        } else if (genericInteractionVFX != null) {
            Instantiate(genericInteractionVFX, transform.position, Quaternion.identity);
        }
    }
}