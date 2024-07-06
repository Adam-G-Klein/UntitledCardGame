using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion : Entity, ICombatStats, IDeckEntity 
{
    public CompanionTypeSO companionType;

    public Deck deck;
    public CombatStats combatStats;
    public int companionLevel = 1;

    public Companion(CompanionTypeSO companionType) 
    {
        this.companionType = companionType;
        this.combatStats = new CombatStats(
                companionType.maxHealth);
        this.deck = new Deck(companionType);
        this.id = Id.newGuid();
        this.entityType = EntityType.Companion;
    }

    public Sprite getSprite() {
        return this.companionType.sprite;
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

    public void Upgrade(List<Companion> companionsCombined) {

        // if this upgrade initiated from combining companions go ahead and replace the current deck with a superset of all the other companions
        if (companionsCombined != default && (companionsCombined.Count > 0)) {
            deck.cards.Clear();
            foreach (var companion in companionsCombined) {
                deck.AddCards(companion.deck);
            }
        }

        //reset health to the new max
        this.combatStats.currentHealth = this.combatStats.maxHealth;
        // temp
        this.combatStats.maxHealth += 40;
        this.deck.cardsDealtPerTurn += 1;
        this.companionLevel += 1;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }
}
