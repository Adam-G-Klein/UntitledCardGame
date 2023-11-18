using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Minion : Entity, ICombatStats, IDeckEntity
{
    public MinionTypeSO minionType;

    public CombatStats combatStats;
    public Deck deck;

    [SerializeReference]
    public CompanionAbility ability;

    public Minion(MinionTypeSO minionType) 
    {
        this.minionType = minionType;
        this.combatStats = new CombatStats(
                minionType.maxHealth,
                minionType.baseAttackDamage);
        this.deck = new Deck(minionType.startingDeck);
        this.ability = minionType.ability;
        this.id = Id.newGuid();
        this.entityType = EntityType.Minion;
    }

    public Deck getDeck() {
        return this.deck;
    }

    public int getDealtPerTurn() {
        return this.deck.cardsDealtPerTurn;
    }

    public void setDealtPerTurn(int dealtPerTurn) {
        this.deck.cardsDealtPerTurn = dealtPerTurn;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }
}
