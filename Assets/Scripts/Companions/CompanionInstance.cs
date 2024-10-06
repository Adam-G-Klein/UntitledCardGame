using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CombatInstance))]
[RequireComponent(typeof(DeckInstance))]
[RequireComponent(typeof(Targetable))]
public class CompanionInstance : MonoBehaviour
{
    public Companion companion;
    [Header("Image or SpriteRenderer required in children")]
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private IEnumerable companionAbilityDeathCallback;
    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    private BoxCollider2D boxCollider2D;

    public void Setup(WorldPositionVisualElement wpve, Companion companion) {
        
        // ---- set some local member variables ----
        this.companion = companion;
        gameObject.name = companion.companionType.name;
        this.combatInstance = GetComponent<CombatInstance>();
        this.deckInstance = GetComponent<DeckInstance>();
        // ---- set up the combatInstance, which has all the logic this shares with all companions/enemies ----
        combatInstance.Setup(companion.combatStats, CombatInstance.CombatInstanceParent.COMPANION, wpve);
        Debug.Log("CompanionInstance Start for companion " + companion.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        combatInstance.onDeathHandler += OnDeath;
        combatInstance.genericInteractionSFX = companion.companionType.genericCompanionSFX;
        combatInstance.genericInteractionVFX = companion.companionType.genericCompanionVFX;
        // ---- set up the deck for this entity ----
        deckInstance.sourceDeck = companion.deck;
        // ---- set up the sprite for this entity in the world ----
        GetComponentInChildren<CombatInstanceDisplayWorldspace>().Setup(combatInstance, wpve);
        // ---- set up status effects turn triggers, so they update when turn phases change ----
        RegisterUpdateStatusEffects();


        // ---- set up abilities ----
        // We cannot perform "Setup" on the ability itself, because that is global on the
        // CompanionTypeSO.
        // If you have multiple copies of the same companion type on the team, they would
        // all try to write state to the same Ability class.
        // Thus, we do this hack around for now where we create a "CompanionAbilityInstance"
        // that has a read-only reference to the Ability but keeps its own state.
        foreach (CompanionAbility ability in companion.companionType.abilities) {
            CompanionAbilityInstance abilityInstance = new(ability, this);
            abilityInstance.Setup();
        }
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
                StatusEffectType.Invulnerability})
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Orb})
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffectType> {
                StatusEffectType.Weakness})
        ));
        statusEffectTriggers.ForEach(trigger => TurnManager.Instance.addTurnPhaseTrigger(trigger));
    }

    private void UnregisterUpdateStatusEffects() {
        statusEffectTriggers.ForEach(trigger => TurnManager.Instance.removeTurnPhaseTrigger(trigger));
    }

    public IEnumerator OnDeath(CombatInstance killer)
    {
        if (companionAbilityDeathCallback != null) {
            yield return StartCoroutine(companionAbilityDeathCallback.GetEnumerator());
        }
        UnregisterUpdateStatusEffects();
        CombatEntityManager.Instance.CompanionDied(this);
    }

    public void SetCompanionAbilityDeathCallback(IEnumerable callback) {
        this.companionAbilityDeathCallback = callback;
    }
}

