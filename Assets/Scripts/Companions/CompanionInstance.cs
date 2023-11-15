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
        companion.ability.Setup(this);
        CombatEntityManager.Instance.registerCompanion(this);
        spriteImage.sprite = companion.getSprite();
        combatInstance.combatStats = companion.combatStats;
        combatInstance.onDeathHandler += OnDeath;
        deckInstance.sourceDeck = companion.deck;
        RegisterUpdateStatusEffects();
    }

    private void RegisterUpdateStatusEffects() {
        statusEffectTriggers.Add(new TurnPhaseTrigger(
            TurnPhase.END_ENEMY_TURN,
            combatInstance.UpdateStatusEffects(new List<StatusEffect> {
                StatusEffect.Defended,
                StatusEffect.TemporaryStrength,
                StatusEffect.Invulnerability })
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

