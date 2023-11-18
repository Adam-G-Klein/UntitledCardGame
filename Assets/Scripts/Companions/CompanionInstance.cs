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

    private TurnPhaseTrigger statusEffectTrigger;

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
        statusEffectTrigger = new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            combatInstance.UpdateStatusEffects());
        TurnManager.Instance.addTurnPhaseTrigger(statusEffectTrigger);
    }

    private void UnregisterUpdateStatusEffects() {
        TurnManager.Instance.removeTurnPhaseTrigger(statusEffectTrigger);
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

