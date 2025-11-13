using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

[System.Serializable]
public class CacheConfiguration {
    public string key;
    public bool display = true;

    public Sprite sprite;

    public int startOfTurnValue;
    public bool setStartOfTurnValue = false;

    public int startOfCombatValue;
    public bool setStartOfCombatValue = false;

    public DisplayedCacheValue CreateDisplay(int curValue) {
        DisplayedCacheValue x = new();
        x.key = key;
        x.value = curValue;
        x.sprite = sprite;
        return x;
    }
}

public class DisplayedCacheValue {
    public string key;
    public int value;
    public Sprite sprite;
}

public class CombatInstance : MonoBehaviour
{
    public CombatStats combatStats;

    public delegate IEnumerator OnDeathHandler(CombatInstance killer);
    public event OnDeathHandler onDeathHandler;


    public delegate void OnDamageHandler(int scale);
    public event OnDamageHandler onDamageHandler;

    public delegate void OnStatusEffectChangeHandler();
    public event OnStatusEffectChangeHandler onStatusEffectChangeHandler;

    public delegate void UpdateViewHandler();
    public event UpdateViewHandler updateViewHandler;
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
            { StatusEffectType.MoneyOnDeath, 0 },
            { StatusEffectType.Charge, 0},
            { StatusEffectType.MaxBlockToLoseAtEndOfTurn, -1 }, // 0 is a valid value so we need to use -1 to indicate a value hasn't been set
            { StatusEffectType.Burn, 0},
            { StatusEffectType.ExtraCardsToDealNextTurn, 0}
        };

    private Dictionary<StatusEffectType, int> statusEffects =
        new Dictionary<StatusEffectType, int> (initialStatusEffects);
    private StatusEffectsDisplay statusEffectsDisplay;

    private PowerPool powers = new();

    // cachedEffectValues is used to persist state across the entire combat.
    // For example, if an enemy needs to count the number of cards played in
    // a given turn, they can load and store a value here.
    public EffectDocument cachedEffectValues;
    public List<CacheConfiguration> cacheConfigs = new();
    TurnPhaseTrigger startOfTurnCacheInit;
    TurnPhaseTrigger startOfCombatCacheInit;

    public WorldPositionVisualElement wpve;
    // Link the GameObject to the visual element. This is necessary now that
    // basically the entire combat screen is UI Doc and we're gonna be hiding
    // the sprites on the game objects that sit on the scene.
    private VisualElement rootVisualElement;
    private bool destroyOnDeath = true;

    public void ApplyStatusEffects(StatusEffectType statusEffect, int scale) {
        Debug.Log(String.Format("Applying status with scale {0}", scale));
        if (parentType == CombatInstanceParent.COMPANION && statusEffect == StatusEffectType.Defended) {
            StartCoroutine(CombatEntityManager.Instance.OnBlockGained(this));
        }
        if (parentType == CombatInstanceParent.COMPANION && (statusEffect == StatusEffectType.Strength || statusEffect == StatusEffectType.TemporaryStrength)) {
            PlayerHand.Instance.UpdatePlayableCards(GetCompanionInstance().deckInstance);
        }
        if (statusEffect == StatusEffectType.BonusBlock)
        {
            statusEffects[StatusEffectType.Defended] += scale;
        }
        else if (statusEffect == StatusEffectType.MaxBlockToLoseAtEndOfTurn)
        {
            statusEffects[statusEffect] = statusEffects[statusEffect] == initialStatusEffects[statusEffect]
                ? scale
                : Mathf.Min(scale, statusEffects[statusEffect]);
            Debug.LogError(statusEffect);
            Debug.LogError(statusEffects[statusEffect]);
        }
        else
        {
            statusEffects[statusEffect] += scale;
        }
        if(statusEffect != StatusEffectType.Defended && statusEffect != StatusEffectType.Orb && statusEffect != StatusEffectType.Strength && statusEffect != StatusEffectType.TemporaryStrength) {
            //PlaySFX();
            MusicController.Instance.PlaySFX("event:/SFX/SFX_NegativeEffect");
            AddVFX();
        }
        else
        {
            MusicController.Instance.PlaySFX("event:/SFX/SFX_PositiveEffect");
        }
        UpdateView();
        onStatusEffectChangeHandler?.Invoke();
    }

    public void Setup(
        CombatStats combatStats,
        Entity parentEntity,
        CombatInstanceParent parentType,
        WorldPositionVisualElement wpve,
        List<CacheConfiguration> cacheConfigs = null,
        bool destroyOnDeath = true
    ) {
        this.combatStats = combatStats;
        this.parentType = parentType;
        this.parentEntity = parentEntity;
        if (combatStats.getCurrentHealth() == 0) {
            combatStats.setCurrentHealth(1);
        }
        // Clear out the cached effect values.
        this.cachedEffectValues = new EffectDocument();
        this.statusEffectsDisplay = GetComponent<StatusEffectsDisplay>();
        if (cacheConfigs != null) {
            this.cacheConfigs = cacheConfigs;
        }
        // Set up turn triggers to store values in the cache effect document at the start of combat
        // or the start of the turn.
        startOfCombatCacheInit = new TurnPhaseTrigger(TurnPhase.START_ENCOUNTER, startOfCombatCacheInitCoroutine());
        TurnManager.Instance.addTurnPhaseTrigger(startOfCombatCacheInit);
        startOfTurnCacheInit = new TurnPhaseTrigger(TurnPhase.BEFORE_START_PLAYER_TURN, startOfTurnCacheInitCoroutine());
        TurnManager.Instance.addTurnPhaseTrigger(startOfTurnCacheInit);

        // null if boss that doesn't have status display
        if(statusEffectsDisplay != null) statusEffectsDisplay.Setup(this, wpve);       
        this.wpve = wpve;
        this.destroyOnDeath = destroyOnDeath;
    }

    public int GetCurrentDamage() {
        return Mathf.Max(0, (
                combatStats.baseAttackDamage
                + statusEffects[StatusEffectType.Strength]
                + statusEffects[StatusEffectType.TemporaryStrength]
                - statusEffects[StatusEffectType.Weakness]));
    }


    public void ApplyNonStatusCombatEffect(CombatEffect effect, int scale, CombatInstance effector, GameObject vfxPrefab, bool shouldShake = false, float screenShakeForce = 0.0f) {
        // All the non-status-effect combat effects are handled here
        // status effects are handled in applyCombatEffects
        if (vfxPrefab != null) {
            PlayVFX(vfxPrefab);
        }
        switch (effect)
        {
            case CombatEffect.Damage:
            case CombatEffect.FixedDamageWithCardModifications:
            case CombatEffect.FixedDamageThatIgnoresBlock:
                int damageTaken = TakeDamage(effect, scale, effector);
                if (screenShakeForce > 0.0f)
                {
                    ScreenShakeManager.Instance.ShakeWithForce(screenShakeForce);
                }
                if (effector.GetComponent<CompanionInstance>() != null)
                {
                    MusicController.Instance.PlaySFX("event:/SFX/SFX_BasicAttack");
                }
                else if (effector.GetComponent<EnemyInstance>() != null)
                {
                    if (damageTaken <= 0 && !killed)
                    {
                        MusicController.Instance.PlaySFX("event:/SFX/SFX_FullBlock");
                    }
                    else
                    {
                        MusicController.Instance.PlaySFX("event:/SFX/SFX_EnemyAttack");
                    }
                }
                if (shouldShake) AddShake(damageTaken);
                if (damageTaken > 0) EnemyEncounterManager.Instance.DamageIndicator(this, damageTaken);
                break;
            case CombatEffect.Heal:
                MusicController.Instance.PlaySFX("event:/SFX/SFX_PositiveEffect");
                int diff = combatStats.Heal(scale);
                if (diff > 0)
                {
                    StartCoroutine(CombatEntityManager.Instance.OnHeal(this));
                }
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
            Debug.Log("TakeDamage called with 'killed=true'; this can habit on multihit moves, returning early.");
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
            attacker.ApplyNonStatusCombatEffect(CombatEffect.FixedDamageWithCardModifications, statusEffects[StatusEffectType.Thorns], this, null);
        }
        // could easily double-update with method above
        UpdateView();
        // return combatStats.getCurrentHealth() == 0 ? 0 : damageAfterDefense;
        return damageAfterDefense;
    }

    private int DamageAfterDefense(CombatEffect combatEffect, int damage)
    {
        // Invulnerability removal is handled at end of turn
        if (statusEffects[StatusEffectType.Invulnerability] > 0)
            return 0;

        // This is commonly used for pay HP as a cost cards.
        if (combatEffect == CombatEffect.FixedDamageThatIgnoresBlock)
        {
            return damage;
        }

        if (statusEffects[StatusEffectType.PlatedArmor] > 0)
        {
            // We have plated armor, so set damage and remove 1 armor
            damage = 0;
            statusEffects[StatusEffectType.PlatedArmor] -= 1;
        }

        // No block, so just taking full damage
        if (statusEffects[StatusEffectType.Defended] == 0)
            return damage;

        if (statusEffects[StatusEffectType.Defended] > damage)
        {
            statusEffects[StatusEffectType.Defended] -= damage;
            damage = 0;
        }
        else if (statusEffects[StatusEffectType.Defended] < damage)
        {
            damage -= statusEffects[StatusEffectType.Defended];
            statusEffects[StatusEffectType.Defended] = 0;
        }
        else
        {
            statusEffects[StatusEffectType.Defended] = 0;
            damage = 0;
        }
        UpdateView();

        return damage;
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        Debug.Log("OnDeath called for " + this.id + " with killer " + killer.GetId());
        Debug.Log("OnDeath: waiting for all effects running to resolve");
        killed = true;
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);
        string blockerId = Id.newGuid();
        TurnManager.Instance.addTurnPhaseBlocker(blockerId);
        ProcessOnDeathStatusEffects(killer);

        // Remove the turn phase triggers for start of combat and start of turn.
        TurnManager.Instance.removeTurnPhaseTrigger(startOfCombatCacheInit);
        TurnManager.Instance.removeTurnPhaseTrigger(startOfTurnCacheInit);

        if (onDeathHandler != null) {
            foreach (OnDeathHandler handler in onDeathHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke(killer));
            }
        }
        // CombatEntityManager.Instance.combatEntityDied(this);
        TurnManager.Instance.removeTurnPhaseBlocker(blockerId);
        UpdateView();
        if (parentType == CombatInstanceParent.COMPANION)
        {
            MusicController.Instance.PlaySFX("event:/MX/MX_CompanionDeath");
        }
        CombatEntityManager.Instance.SpawnEntityOnDeathVfx(this);
        if (destroyOnDeath) Destroy(this.gameObject);
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
        onStatusEffectChangeHandler?.Invoke();
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

    public bool ActivatePower(PowerSO power)
    {
        bool activated = powers.ActivatePower(power);
        UpdateView();
        return activated;
    }

    public List<PowerSO> GetUniquePowers()
    {
        return powers.GetPowers()
            .GroupBy(p => p.powerType)
            .Select(g => g.First())
            .ToList();
    }

    public List<(PowerSO, int)> GetPowersWithStackCounts()
    {
        return powers.GetPowers()
            .GroupBy(p => p.powerType)
            .Select(g => (g.First(), g.Count()))
            .ToList();
    }

    public int GetNumStackOfPower(PowerSO.PowerType powerType)
    {
        if (!powers.HasPower(powerType))
        {
            return 0;
        }
        return GetPowersWithStackCounts().Where(x => x.Item1.powerType == powerType).First().Item2;
    }

    public bool HasPower(PowerSO.PowerType powerType)
    {
        return powers.HasPower(powerType);
    }

    public CompanionInstance GetCompanionInstance()
    {
        switch (parentType)
        {
            case CombatInstanceParent.COMPANION:
                return GetComponent<CompanionInstance>();
            default:
                return null;
        }
    }

    private void UpdateStatusEffect(StatusEffectType status) {
        switch (status)
        {
            case StatusEffectType.Defended:
                if (HasPower(PowerSO.PowerType.Barricade))
                {
                    break;
                }
                else if (statusEffects[StatusEffectType.MaxBlockToLoseAtEndOfTurn] != initialStatusEffects[StatusEffectType.MaxBlockToLoseAtEndOfTurn])
                {
                    statusEffects[status] = Mathf.Max(0, statusEffects[status] - statusEffects[StatusEffectType.MaxBlockToLoseAtEndOfTurn]);
                }
                else
                {
                    statusEffects[status] = 0;
                }
                break;
            case StatusEffectType.MaxBlockToLoseAtEndOfTurn:
                statusEffects[status] = -1;
                break;
            case StatusEffectType.Thorns:
                statusEffects[status] = 0;
                break;
            case StatusEffectType.ExtraCardsToDealNextTurn:
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
            case StatusEffectType.Charge:
                if (statusEffects[status] > 0)
                {
                    statusEffects[status] = DecrementStatus(statusEffects[status]);
                    ApplyStatusEffects(StatusEffectType.Defended, 3);
                }
                break;
            case StatusEffectType.Orb:
                break;
            case StatusEffectType.Burn:
                // Debug.LogError(statusEffects[status]);
                if (statusEffects[status] > 0)
                {
                    // ideally we would have a map of status effects to their vfx prefab (or some other way of instantiating them...this will be pretty opaque without vfx)
                    ApplyNonStatusCombatEffect(CombatEffect.FixedDamageThatIgnoresBlock, statusEffects[status], this, null, true);
                    statusEffects[status] = DecrementStatus(statusEffects[status]);
                }
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

    private void PlayVFX(GameObject vfxPrefab) {
        if (vfxPrefab != null) Instantiate(vfxPrefab, transform.position, Quaternion.identity);
    }

    private void AddShake(int damageTaken) {
        onDamageHandler.Invoke(damageTaken);
    }

    private void UpdateView() {
        EnemyEncounterViewModel.Instance.SetStateDirty();
        updateViewHandler?.Invoke();
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

    public void SetVisualElement(VisualElement element) {
        Debug.Log(String.Format("Setting rootVisualElement {0} on CombatInstance {1} (type {2})", element, this.id, this.parentType));
        this.rootVisualElement = element;
    }

    public VisualElement GetVisualElement() {
        return this.rootVisualElement;
    }

    public Dictionary<StatusEffectType, int> GetDisplayedStatusEffects() {
        Dictionary<StatusEffectType, int> displayedStatusEffects = new Dictionary<StatusEffectType, int>();
        foreach (KeyValuePair<StatusEffectType, int> statusEffect in statusEffects) {
            int value = statusEffect.Value;
            if (statusEffect.Key == StatusEffectType.Strength) {
                value += combatStats.baseAttackDamage;
            }
            // block is queried separately now
            // ^^ Get Undo-d goober
            // if (value != initialStatusEffects[statusEffect.Key]
            //     && statusEffect.Key != StatusEffectType.Defended)
            if (value != initialStatusEffects[statusEffect.Key])
            {
                displayedStatusEffects.Add(statusEffect.Key, value);
            }
        }
        return displayedStatusEffects;
    }

    public List<DisplayedCacheValue> GetDisplayedCacheValues() {
        List<DisplayedCacheValue> displayed = new();
        foreach (CacheConfiguration cacheConfig in cacheConfigs) {
            if (!cacheConfig.display) {
                continue;
            }
            int curValue = this.cachedEffectValues.intMap.GetValueOrDefault(cacheConfig.key);
            displayed.Add(cacheConfig.CreateDisplay(curValue));
        }
        return displayed;
    }

    private IEnumerable startOfCombatCacheInitCoroutine() {
        foreach (CacheConfiguration config in this.cacheConfigs) {
            if (config.setStartOfCombatValue) {
                this.cachedEffectValues.intMap[config.key] = config.startOfCombatValue;
            }
        }
        yield return null;
    }

    private IEnumerable startOfTurnCacheInitCoroutine() {
        foreach (CacheConfiguration config in this.cacheConfigs) {
            if (config.setStartOfTurnValue) {
                this.cachedEffectValues.intMap[config.key] = config.startOfTurnValue;
            }
        }
        yield return null;
    }

    public enum CombatInstanceParent {
        COMPANION,
        ENEMY
    }
}