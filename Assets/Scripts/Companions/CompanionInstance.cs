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
    public Image spriteImage;
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private IEnumerable companionAbilityDeathCallback;

    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    public void Start() {
        // companion.companionType.ability.Setup(this);
        foreach (CompanionAbility ability in companion.companionType.abilities) {
            ability.Setup(this);
        }
        CombatEntityManager.Instance.registerCompanion(this);
        spriteImage.sprite = companion.getSprite();
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

