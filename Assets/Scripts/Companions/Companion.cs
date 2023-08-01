using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion: CombatEntityBaseStats, CombatEntityWithDeck 
{
    public CompanionTypeSO companionType;
    public int maxHealth;
    public int currentHealth;
    public Deck deck;
    public int baseAttackDamage;
    public CompanionUpgradeSO upgradeInfo;
    public int cardsDealtPerTurn = 1;

    [SerializeReference]
    public CompanionAbility ability;
    // This might look like a duplicate id field from the one 
    // that all Entities have. I thought it was. But we need it
    // to keep track of the companion between encounters.
    // So we'll just set the id on the Entity this becomes in the 
    // scene to be the same as this one.
    public string id;
    public Companion(CompanionTypeSO companionType) 
    {
        this.companionType = companionType;
        this.maxHealth = companionType.maxHealth;
        this.currentHealth = this.maxHealth;
        this.baseAttackDamage = companionType.baseAttackDamage;
        this.deck = new Deck(companionType.startingDeck);
        this.ability = companionType.ability;
        this.id = Id.newGuid();
        this.upgradeInfo = companionType.upgradeInfo;
    }



    public int getBaseAttackDamage() {
        return this.baseAttackDamage;
    }

    public int getMaxHealth() {
        return this.maxHealth;
    }

    public void setMaxHealth(int newMaxHealth) {
        this.maxHealth = newMaxHealth;
    }

    public int getCurrentHealth() {
        return this.currentHealth;
    }

    public void setCurrentHealth(int newHealth) {
        this.currentHealth = newHealth;
    }

    public EntityType getEntityType() {
        return EntityType.Companion;
    }

    public string getId() {
        return this.id;
    }

    public Sprite getSprite() {
        return this.companionType.sprite;
    }

    public Deck getDeck() {
        return this.deck;
    }

    public void setBaseAttackDamage(int newBaseStrength) {
        this.baseAttackDamage = newBaseStrength;
    }

    public int getDealtPerTurn() {
        return this.cardsDealtPerTurn;
    }

    public void setDealtPerTurn(int dealtPerTurn) {
        this.cardsDealtPerTurn = dealtPerTurn;
    }

    public void Upgrade(List<Companion> companionsCombined) {
        Debug.Assert(upgradeInfo != default, "There is no information on upgrading, please add the \"CompanionUpgradeSO\" Scriptable Object");

        //if this upgrade initiated from combining companions go ahead and replace the current deck with a superset of all the other companions
        if (companionsCombined != default && (companionsCombined.Count > 0)) {
            deck.cards.Clear();
            foreach (var companion in companionsCombined) {

                deck.addCards(companion.deck);
            }
        }

        //reset health to the new max
        this.maxHealth *= upgradeInfo.healthUpgradeFactor;
        this.currentHealth = this.maxHealth;
        
        this.cardsDealtPerTurn += upgradeInfo.cardPerTurnUpgrade;
    }
}
