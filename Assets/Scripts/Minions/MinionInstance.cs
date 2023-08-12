using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionInstance : MonoBehaviour
{
    public Minion minion;
    public CombatInstance combatInstance;
    public DeckInstance deckInstance;

    private TurnPhaseTrigger statusEffectTrigger;

    public void Start() {
        CombatEntityManager.Instance.registerMinion(this);
        combatInstance.combatStats = minion.combatStats;
        deckInstance.sourceDeck = minion.deck;
        combatInstance.onDeathHandler += OnDeath;
        RegisterUpdateStatusEffects();
    }

    private void RegisterUpdateStatusEffects() {
        statusEffectTrigger = new TurnPhaseTrigger(
            TurnPhase.END_ENEMY_TURN,
            combatInstance.UpdateStatusEffects());
        TurnManager.Instance.addTurnPhaseTrigger(statusEffectTrigger);
    }

    private void UnregisterUpdateStatusEffects() {
        TurnManager.Instance.removeTurnPhaseTrigger(statusEffectTrigger);
    }

    public IEnumerator OnDeath(CombatInstance killer)
    {
        UnregisterUpdateStatusEffects();
        CombatEntityManager.Instance.MinionDied(this);
        yield return null;
    }
}

