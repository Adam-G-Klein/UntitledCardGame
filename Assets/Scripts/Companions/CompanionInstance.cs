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
    public Image spriteImage;
    public SpriteRenderer spriteRenderer;
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private IEnumerable companionAbilityDeathCallback;
    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    private BoxCollider2D boxCollider2D;

    public void Start() {
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
        CombatEntityManager.Instance.registerCompanion(this);

        combatInstance.parentType = CombatInstance.CombatInstanceParent.COMPANION;
        combatInstance.combatStats = companion.combatStats;
        Debug.Log("CompanionInstance Start for companion " + companion.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        if (combatInstance.combatStats.getCurrentHealth() == 0) {
            combatInstance.combatStats.setCurrentHealth(1);
        }
        combatInstance.onDeathHandler += OnDeath;
        combatInstance.genericInteractionSFX = companion.companionType.genericCompanionSFX;
        combatInstance.genericInteractionVFX = companion.companionType.genericCompanionVFX;
        deckInstance.sourceDeck = companion.deck;
        RegisterUpdateStatusEffects();
    }

    public void Setup(WorldPositionVisualElement wpve, Companion companion) {
        this.companion = companion;
        gameObject.name = companion.companionType.name;
        this.combatInstance = GetComponent<CombatInstance>();
        this.deckInstance = GetComponent<DeckInstance>();
        combatInstance.parentType = CombatInstance.CombatInstanceParent.COMPANION;
        combatInstance.combatStats = companion.combatStats;
        Debug.Log("CompanionInstance Start for companion " + companion.id + " initialized with combat stats (health): " + combatInstance.combatStats.getCurrentHealth());
        if (combatInstance.combatStats.getCurrentHealth() == 0) {
            combatInstance.combatStats.setCurrentHealth(1);
        }
        combatInstance.onDeathHandler += OnDeath;
        combatInstance.genericInteractionSFX = companion.companionType.genericCompanionSFX;
        combatInstance.genericInteractionVFX = companion.companionType.genericCompanionVFX;
        deckInstance.sourceDeck = companion.deck;
        GetComponentInChildren<CombatInstanceDisplayWorldspace>().Setup(combatInstance, wpve);
        RegisterUpdateStatusEffects();

    }

    private void RegisterUpdateStatusEffects() {
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_ENEMY_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffect> {
                StatusEffect.Defended,
                StatusEffect.TemporaryStrength,
                StatusEffect.Invulnerability})
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffect> {
                StatusEffect.Orb})
        ));
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffect> {
                StatusEffect.Weakness})
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

