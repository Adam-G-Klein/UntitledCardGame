using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Minion: CombatEntityBaseStats, CombatEntityWithDeck
{
    public MinionTypeSO minionType;
    public int maxHealth;
    public int currentHealth;
    public Deck deck;
    public int baseAttackDamage;
    public int cardsDealtPerTurn = 1;

    [SerializeReference]
    public List<CompanionAbility> abilities;
    // This might look like a duplicate id field from the one 
    // that all Entities have. I thought it was. But we need it
    // to keep track of the companion between encounters.
    // So we'll just set the id on the Entity this becomes in the 
    // scene to be the same as this one.
    public string id;
    public Minion(MinionTypeSO minionType) 
    {
        this.minionType = minionType;
        this.maxHealth = minionType.maxHealth;
        this.currentHealth = this.maxHealth;
        this.baseAttackDamage = minionType.baseAttackDamage;
        this.deck = new Deck(minionType.startingDeck);
        this.abilities = minionType.abilities;
        this.id = Id.newGuid();
        Debug.Log(JsonUtility.ToJson(this));
    }

    public int getBaseAttackDamage() {
        return this.baseAttackDamage;
    }

    public void setBaseAttackDamage(int newBaseAttackDamage) {
        this.baseAttackDamage = newBaseAttackDamage;
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
        return this.minionType.sprite;
    }

    public Deck getDeck() {
        return this.deck;
    }
    public int getDealtPerTurn() {
        return this.cardsDealtPerTurn;
    }

    public void setDealtPerTurn(int dealtPerTurn) {
        this.cardsDealtPerTurn = dealtPerTurn;
    }
}
