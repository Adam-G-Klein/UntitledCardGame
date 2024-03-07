using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion : Entity, ICombatStats, IDeckEntity 
{
    public CompanionTypeSO companionType;
    public CompanionUpgradeSO upgradeInfo;

    public Deck deck;
    public CombatStats combatStats;

    public Companion(CompanionTypeSO companionType) 
    {
        this.companionType = companionType;
        this.combatStats = new CombatStats(
                companionType.maxHealth);
        this.deck = new Deck(companionType.startingDeck, companionType.initialCardsDealtPerTurn);
        this.id = Id.newGuid();
        this.entityType = EntityType.Companion;
        this.upgradeInfo = companionType.upgradeInfo;
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
        Debug.Assert(upgradeInfo != default, "There is no information on upgrading, please add the \"CompanionUpgradeSO\" Scriptable Object");

        // if this upgrade initiated from combining companions go ahead and replace the current deck with a superset of all the other companions
        if (companionsCombined != default && (companionsCombined.Count > 0)) {
            deck.cards.Clear();
            foreach (var companion in companionsCombined) {
                deck.AddCards(companion.deck);
            }
        }

        //reset health to the new max
        this.combatStats.MultiplyMaxHealth((float) upgradeInfo.healthUpgradeFactor);
        
        this.deck.cardsDealtPerTurn += upgradeInfo.cardPerTurnUpgrade;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }
}
