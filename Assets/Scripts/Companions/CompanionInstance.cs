using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(DeckInstance))]
[RequireComponent(typeof(Targetable))]
[RequireComponent(typeof(CombatCompanionTooltipProvder))]
public class CompanionInstance : MonoBehaviour, IUIEntity
{
    public Companion companion;
    [Header("Image or SpriteRenderer required in children")]
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;
    public StatusEffectsDisplay statusEffectsDisplay;
    public CombatCompanionTooltipProvder tooltipProvider;

    // MAY BE NULL if CombatEncounterView.ResetEntities hasn't been called yet
    // That method is only called AFTER the encountermanager has queried the locations
    // of all of the ui elements in the ui doc and placed the companionInstances in their
    // proper location on the screen
    public CompanionView companionView;

    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    private BoxCollider2D boxCollider2D;

    public void Setup(WorldPositionVisualElement wpve, Companion companion)
    {

        // ---- set some local member variables ----
        this.companion = companion;
        gameObject.name = companion.companionType.name;
        this.combatInstance = GetComponent<CombatInstance>();
        this.deckInstance = GetComponent<DeckInstance>();
        // ---- set up the combatInstance, which has all the logic this shares with all companions/enemies ----
        combatInstance.Setup(companion.combatStats, companion, CombatInstance.CombatInstanceParent.COMPANION, wpve, companion.companionType.cacheValueConfigs);
        Debug.Log("CompanionInstance Start for companion " + companion.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        // ---- set up the deck for this entity ----
        deckInstance.sourceDeck = companion.deck;
        // ---- set up the sprite for this entity in the world ----
        // GetComponentInChildren<CombatInstanceDisplayWorldspace>().Setup(combatInstance, wpve);
        // ---- set up status effects turn triggers, so they update when turn phases change ----
        RegisterUpdateStatusEffects();

        combatInstance.onStatusEffectChangeHandler += OnStatusEffectChangeHandler;


        // ---- set up abilities ----
        // We cannot perform "Setup" on the ability itself, because that is global on the
        // CompanionTypeSO.
        // If you have multiple copies of the same companion type on the team, they would
        // all try to write state to the same Ability class.
        // Thus, we do this hack around for now where we create a "CompanionAbilityInstance"
        // that has a read-only reference to the Ability but keeps its own state.
        foreach (EntityAbility ability in companion.companionType.abilitiesV2)
        {
            CompanionInstanceAbilityInstance abilityInstance = new(ability, this);
            abilityInstance.Setup();
        }
        // Register the OnDeath handler after we initialize the abilities, so that the ability triggers go off before the
        // companion dies altogether.
        combatInstance.onDeathHandler += OnDeath;
        // ---- register with the manager, which will track things like whether there's no companions or enemies left when
        // one dies and end the encounter ----
        CombatEntityManager.Instance.registerCompanion(this);
    }

    public void ActivatePower(PowerSO power)
    {
        bool activated = combatInstance.ActivatePower(power);
        // If it's activated, then we will set up the abilities on the entity ability instance.
        if (activated)
        {
            Debug.Log($"Ability activated for power {power.name}");
            foreach (EntityAbility ability in power.abilities)
            {
                CompanionInstanceAbilityInstance abilityInstance = new(ability, this);
                abilityInstance.Setup();
            }
        }
        List<(PowerSO, int)> powersWithStacks = combatInstance.GetPowersWithStackCounts();
        List<(PowerSO, int)> selected = powersWithStacks.Where(p => p.Item1.powerType == power.powerType).ToList();
        // If it's stackable, we may have multiple instances of it, so rmeove the first tooltip.
        if (selected.Count >= 1 && selected.First().Item2 > 1)
        {
            TooltipViewModel b = power.GetTooltip(selected.First().Item2 - 1);
            foreach (var line in b.lines)
            {
                tooltipProvider.RemoveTooltipTitleRegexp(line.title);
            }
        }
        tooltipProvider.AddTooltip(power.GetTooltip(selected.First().Item2));
    }

    private void RegisterUpdateStatusEffects()
    {
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_ENEMY_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Defended,
                StatusEffectType.TemporaryStrength,
                StatusEffectType.Invulnerability,
                StatusEffectType.MaxBlockToLoseAtEndOfTurn}) // needs to be after defended to ensure block is maintained
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Orb,
                StatusEffectType.Burn,
                StatusEffectType.ExtraCardsToDealNextTurn,
            })
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Weakness, StatusEffectType.Charge})
        ));
        statusEffectTriggers.ForEach(trigger => TurnManager.Instance.addTurnPhaseTrigger(trigger));
    }

    private void UnregisterUpdateStatusEffects()
    {
        statusEffectTriggers.ForEach(trigger => TurnManager.Instance.removeTurnPhaseTrigger(trigger));
    }

    private void OnStatusEffectChangeHandler() {
        tooltipProvider.UpdateStatusTooltips(combatInstance.GetDisplayedStatusEffects(), statusEffectsDisplay.statusEffectsSO.statusEffects);
    }

    public IEnumerator OnDeath(CombatInstance killer)
    {
        UnregisterUpdateStatusEffects();
        CombatEntityManager.Instance.CompanionDied(this);
        companion.trackingStats.RecordDeath();
        yield break;
    }

    public void SetCompanionAbilityDeathCallback(IEnumerable callback)
    {
    }

    public string GetName()
    {
        return companion.GetName();
    }

    public int GetCurrentHealth()
    {
        return combatInstance.combatStats.getCurrentHealth();
    }

    public string GetDescription()
    {
        return companion.companionType.keepsakeDescription;
    }

    public CombatStats GetCombatStats()
    {
        return combatInstance.combatStats;
    }

    public CombatInstance GetCombatInstance()
    {
        return combatInstance;
    }

    public EnemyInstance GetEnemyInstance()
    {
        return null;
    }

    public DeckInstance GetDeckInstance()
    {
        return deckInstance;
    }

    public Targetable GetTargetable()
    {
        return GetComponent<Targetable>();
    }

    public DisplayType GetDisplayType()
    {
        return DisplayType.UIDOC;
    }
}

