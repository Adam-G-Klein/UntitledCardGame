using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(DeckInstance))]
[RequireComponent(typeof(Targetable))]
public class CompanionInstance : MonoBehaviour, IUIEntity
{
    public Companion companion;
    [Header("Image or SpriteRenderer required in children")]
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    private BoxCollider2D boxCollider2D;

    public void Setup(WorldPositionVisualElement wpve, Companion companion) {

        // ---- set some local member variables ----
        this.companion = companion;
        gameObject.name = companion.companionType.name;
        this.combatInstance = GetComponent<CombatInstance>();
        this.deckInstance = GetComponent<DeckInstance>();
        // ---- set up the combatInstance, which has all the logic this shares with all companions/enemies ----
        combatInstance.Setup(companion.combatStats, companion, CombatInstance.CombatInstanceParent.COMPANION, wpve, companion.companionType.cacheValueConfigs);
        Debug.Log("CompanionInstance Start for companion " + companion.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        //combatInstance.genericInteractionSFX = companion.companionType.genericCompanionSFX;
        combatInstance.genericInteractionVFX = companion.companionType.genericCompanionVFX;
        // ---- set up the deck for this entity ----
        deckInstance.sourceDeck = companion.deck;
        // ---- set up the sprite for this entity in the world ----
        // GetComponentInChildren<CombatInstanceDisplayWorldspace>().Setup(combatInstance, wpve);
        // ---- set up status effects turn triggers, so they update when turn phases change ----
        RegisterUpdateStatusEffects();


        // ---- set up abilities ----
        // We cannot perform "Setup" on the ability itself, because that is global on the
        // CompanionTypeSO.
        // If you have multiple copies of the same companion type on the team, they would
        // all try to write state to the same Ability class.
        // Thus, we do this hack around for now where we create a "CompanionAbilityInstance"
        // that has a read-only reference to the Ability but keeps its own state.
        foreach (EntityAbility ability in companion.companionType.abilitiesV2) {
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

    private void RegisterUpdateStatusEffects() {
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

    private void UnregisterUpdateStatusEffects() {
        statusEffectTriggers.ForEach(trigger => TurnManager.Instance.removeTurnPhaseTrigger(trigger));
    }

    public IEnumerator OnDeath(CombatInstance killer)
    {
        UnregisterUpdateStatusEffects();
        CombatEntityManager.Instance.CompanionDied(this);
        yield break;
    }

    public void SetCompanionAbilityDeathCallback(IEnumerable callback) {
    }

    public string GetName() {
        return companion.GetName();
    }

    public int GetCurrentHealth() {
        return combatInstance.combatStats.getCurrentHealth();
    }

    public string GetDescription() {
        return companion.companionType.keepsakeDescription;
    }

    public CombatStats GetCombatStats() {
        return combatInstance.combatStats;
    }

    public CombatInstance GetCombatInstance() {
        return combatInstance;
    }

    public EnemyInstance GetEnemyInstance() {
        return null;
    }

    public DeckInstance GetDeckInstance() {
        return deckInstance;
    }

    public Targetable GetTargetable() {
        return GetComponent<Targetable>();
    }

    public Sprite GetBackgroundImage() {
        return companion.companionType.backgroundImage;
    }

    public Sprite GetEntityFrame() {
        return companion.companionType.entityFrame;
    }
}

