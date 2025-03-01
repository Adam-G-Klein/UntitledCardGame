using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatInstance : MonoBehaviour
{
    public CombatStats combatStats;

    public delegate IEnumerator OnDeathHandler(CombatInstance killer);
    public event OnDeathHandler onDeathHandler;


    public delegate void OnDamageHandler(int scale);
    public event OnDamageHandler onDamageHandler;

    //public AudioClip genericInteractionSFX;
    public GameObject genericInteractionVFX;

    // Only used for debugging purposes, will be set by some other script
    private string id;

    // Distinguish between enemies and companions for combat instances.
    public CombatInstanceParent parentType;

    public bool killed = false;

    public Entity parentEntity;

    public static Dictionary<StatusEffectType, int> initialStatusEffects =
        new Dictionary<StatusEffectType, int>() {
            { StatusEffectType.Strength, 0 },
            { StatusEffectType.Weakness, 0 },
            { StatusEffectType.Defended, 0 },
            { StatusEffectType.DamageMultiply, 1 },
            { StatusEffectType.Invulnerability, 0 },
            { StatusEffectType.MaxHpBounty, 0 },
            { StatusEffectType.TemporaryStrength, 0 },
            { StatusEffectType.MinionsOnDeath, 0 },
            { StatusEffectType.PlatedArmor, 0 },
            { StatusEffectType.Orb, 0 },
            { StatusEffectType.Thorns, 0 },
            { StatusEffectType.MoneyOnDeath, 0 }
        };

    private Dictionary<StatusEffectType, int> statusEffects =
        new Dictionary<StatusEffectType, int> (initialStatusEffects);

    private StatusEffectsDisplay statusEffectsDisplay;

    // cachedEffectValues is used to persist state across the entire combat.
    // For example, if an enemy needs to count the number of cards played in
    // a given turn, they can load and store a value here.
    public EffectDocument cachedEffectValues;

    private WorldPositionVisualElement wpve;

    public void ApplyStatusEffects(StatusEffectType statusEffect, int scale) {
        Debug.Log(String.Format("Applying status with scale {0}", scale));
        statusEffects[statusEffect] += scale;
        if(statusEffect != StatusEffectType.Orb && statusEffect != StatusEffectType.Strength && statusEffect != StatusEffectType.TemporaryStrength) {
            //PlaySFX();
            MusicController2.Instance.PlaySFX("event:/SFX/SFX_NegativeEffect");
            AddVFX();
        }
        else
        {
            MusicController2.Instance.PlaySFX("event:/SFX/SFX_PositiveEffect");
        }
        UpdateView();
    }

    public void Setup(CombatStats combatStats, Entity parentEntity, CombatInstanceParent parentType, WorldPositionVisualElement wpve) {
        this.combatStats = combatStats;
        this.parentType = parentType;
        this.parentEntity = parentEntity;
        if (combatStats.getCurrentHealth() == 0) {
            combatStats.setCurrentHealth(1);
        }
        // Clear out the cached effect values.
        this.cachedEffectValues = new EffectDocument();
        this.statusEffectsDisplay = GetComponent<StatusEffectsDisplay>();
        statusEffectsDisplay.Setup(this, wpve);
        this.wpve = wpve;
    }

    public int GetCurrentDamage() {
        return Mathf.Max(0, (
                combatStats.baseAttackDamage
                + statusEffects[StatusEffectType.Strength]
                + statusEffects[StatusEffectType.TemporaryStrength]
                - statusEffects[StatusEffectType.Weakness]));
    }


    public void ApplyNonStatusCombatEffect(CombatEffect effect, int scale, CombatInstance effector) {

        // All the non-status-effect combat effects are handled here
        // status effects are handled in applyCombatEffects
        switch(effect) {
            case CombatEffect.Damage:
            case CombatEffect.FixedDamageWithCardModifications:
            case CombatEffect.FixedDamageThatIgnoresBlock:
                int damageTaken = TakeDamage(effect, scale, effector);
                if (effector.GetComponent<CompanionInstance>() != null) {
                    MusicController2.Instance.PlaySFX("event:/SFX/SFX_BasicAttack");
                } else if (effector.GetComponent<EnemyInstance>() != null) {
                    MusicController2.Instance.PlaySFX("event:/SFX/SFX_EnemyAttack");
                }
                AddVFX(effector);
                AddShake(damageTaken);
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
            case CombatEffect.SetToHalfHealth:
                combatStats.setCurrentHealth(combatStats.maxHealth / 2);
                break;
            case CombatEffect.Sacrifice:
                StartCoroutine(OnDeath(effector));
                break;
        }
        UpdateView();
    }

    private int TakeDamage(CombatEffect combatEffect, int damage, CombatInstance attacker) {

        if (killed) {
            return 0;
        }
        int damageAfterDefense = DamageAfterDefense(combatEffect, damage);

        if (damageAfterDefense > 0) {
            // If there are on-damage effects from companion abilities,
            // we invoke them here.
            // Note: this only should run if there is damage dealt to the guy.
            StartCoroutine(CombatEntityManager.Instance.OnDamageTaken(this));
        }

        Debug.Log("Take Damage is Setting current health to " + Mathf.Max(combatStats.getCurrentHealth() - damageAfterDefense, 0));
        combatStats.setCurrentHealth(Mathf.Max(combatStats.getCurrentHealth() - damageAfterDefense, 0));

        if (combatStats.getCurrentHealth() == 0){
            StartCoroutine(OnDeath(attacker));
        }

        if (statusEffects[StatusEffectType.Thorns] > 0) {
            attacker.ApplyNonStatusCombatEffect(CombatEffect.FixedDamageWithCardModifications, statusEffects[StatusEffectType.Thorns], this);
        }
        // could easily double-update with method above
        UpdateView();
        return combatStats.getCurrentHealth() == 0 ? 0 : damageAfterDefense;
    }

    private int DamageAfterDefense(CombatEffect combatEffect, int damage) {
        // Invulnerability removal is handled at end of turn
        if (statusEffects[StatusEffectType.Invulnerability] > 0)
            return 0;

        // This is commonly used for pay HP as a cost cards.
        if (combatEffect == CombatEffect.FixedDamageThatIgnoresBlock) {
            return damage;
        }

        if (statusEffects[StatusEffectType.PlatedArmor] > 0) {
            // We have plated armor, so set damage and remove 1 armor
            damage = 0;
            statusEffects[StatusEffectType.PlatedArmor] -= 1;
        }

        // No block, so just taking full damage
        if(statusEffects[StatusEffectType.Defended] == 0)
            return damage;

        if (statusEffects[StatusEffectType.Defended] > damage) {
            statusEffects[StatusEffectType.Defended] -= damage;
            damage = 0;
        } else if (statusEffects[StatusEffectType.Defended] < damage) {
            damage -= statusEffects[StatusEffectType.Defended];
            statusEffects[StatusEffectType.Defended] = 0;
        } else {
            statusEffects[StatusEffectType.Defended] = 0;
            damage = 0;
        }
        UpdateView();

        return damage;
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        string blockerId = Id.newGuid();
        killed = true;
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
        UpdateView();
        CombatEntityManager.Instance.SpawnEntityOnDeathVfx(this);
        Destroy(this.gameObject);
        yield return null;
    }

    // tbh this method could be good reason to rethink how we do status effects.
    // like, should they be able to call their own custom coroutines when something
    // happens to the entity they're on?
    private void ProcessOnDeathStatusEffects(CombatInstance killer) {
        if (killer != null && statusEffects[StatusEffectType.MaxHpBounty] > 0) {
            killer.combatStats.maxHealth += statusEffects[StatusEffectType.MaxHpBounty];
            killer.combatStats.setCurrentHealth(killer.combatStats.getCurrentHealth() + statusEffects[StatusEffectType.MaxHpBounty]);
        }

        if (statusEffects[StatusEffectType.MoneyOnDeath] > 0) {
            PlayerData playerData = EnemyEncounterManager.Instance.gameState.playerData.GetValue();
            playerData.gold += statusEffects[StatusEffectType.MoneyOnDeath];
        }
        UpdateView();
    }

    // This function is setup the way it is because certain statuses need to be
    // updated at different times. Making this function take in a list of statuses
    // allows us to separate when one status is updated vs another
    public IEnumerable UpdateStatusEffects(List<StatusEffectType> statuses) {
        foreach (StatusEffectType effect in statuses) {
            UpdateStatusEffect(effect);
        }
        UpdateView();
        yield return null;
    }

    public Sprite GetSprite() {
        switch(parentType) {
            case CombatInstanceParent.COMPANION:
                return GetComponent<CompanionInstance>().companion.getSprite();
            case CombatInstanceParent.ENEMY:
                return GetComponent<EnemyInstance>().enemy.getSprite();
            default:
                return null;
        }
    }

    public CompanionInstance GetCompanionInstance() {
        switch(parentType) {
            case CombatInstanceParent.COMPANION:
                return GetComponent<CompanionInstance>();
            default:
                return null;
        }
    }

    private void UpdateStatusEffect(StatusEffectType status) {
        switch (status) {
            case StatusEffectType.Defended:
                statusEffects[status] = 0;
                break;
            case StatusEffectType.Thorns:
                statusEffects[status] = 0;
                break;

            case StatusEffectType.TemporaryStrength:
                statusEffects[status] = 0;
                break;
            case StatusEffectType.Invulnerability:
                statusEffects[status] = 0;
                break;
            case StatusEffectType.Weakness:
                statusEffects[status] = DecrementStatus(statusEffects[status]);
                break;
            case StatusEffectType.Orb:
                break;

            // This is separate from the above for now since this might
            // need special logic
            case StatusEffectType.MoneyOnDeath:
                statusEffects[status] = DecrementStatus(statusEffects[status]);
            break;
        }
        UpdateView();
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

    private void AddVFX(CombatInstance effector = null) {
        if(effector != null && effector.genericInteractionVFX != null) {
            Instantiate(effector.genericInteractionVFX, transform.position, Quaternion.identity);
        } else if (genericInteractionVFX != null) {
            Instantiate(genericInteractionVFX, transform.position, Quaternion.identity);
        }
    }

    private void AddShake(int damageTaken) {
        onDamageHandler.Invoke(damageTaken);
    }

    private void UpdateView() {
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    public void SetStatusEffect(StatusEffectType statusEffectType, int value) {
        statusEffects[statusEffectType] = value;
        UpdateView();
    }

    public int GetStatus(StatusEffectType statusEffectType) {
        return statusEffects[statusEffectType];
    }

    public Dictionary<StatusEffectType, int> GetStatusEffects() {
        return statusEffects;
    }

    public Dictionary<StatusEffectType, int> GetDisplayedStatusEffects() {
        Dictionary<StatusEffectType, int> displayedStatusEffects = new Dictionary<StatusEffectType, int>();
        foreach (KeyValuePair<StatusEffectType, int> statusEffect in statusEffects) {
            int value = statusEffect.Value;
            if (statusEffect.Key == StatusEffectType.Strength) {
                value += combatStats.baseAttackDamage;
            }
            // block is queried separately now
            if (value != initialStatusEffects[statusEffect.Key]
                && statusEffect.Key != StatusEffectType.Defended) {
                displayedStatusEffects.Add(statusEffect.Key, value);
            }
        }
        return displayedStatusEffects;
    }

    public enum CombatInstanceParent {
        COMPANION,
        ENEMY
    }
}