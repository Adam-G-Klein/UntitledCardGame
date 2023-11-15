using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionInstance : MonoBehaviour
{
    public Minion minion;
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private List<TurnPhaseTrigger> statusEffectTriggers = new List<TurnPhaseTrigger>();

    public void Start() {
        CombatEntityManager.Instance.registerMinion(this);
        combatInstance.combatStats = minion.combatStats;
        deckInstance.sourceDeck = minion.deck;
        combatInstance.onDeathHandler += OnDeath;
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
        UnregisterUpdateStatusEffects();
        CombatEntityManager.Instance.MinionDied(this);
        yield return null;
    }
}

