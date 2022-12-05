using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion: CombatEntityBaseStats
{
    public CompanionTypeSO companionType;
    public int maxHealth;
    public Deck deck;
    public string id;
    public int baseAttackDamage;
    public Companion(CompanionTypeSO companionType) 
    {
        this.companionType = companionType;
        this.maxHealth = companionType.maxHealth;
        this.baseAttackDamage = companionType.baseAttackDamage;
        this.deck = new Deck(companionType.startingDeck);
        this.id = Id.newGuid();
        Debug.Log(JsonUtility.ToJson(this));
    }

    public int getBaseAttackDamage() {
        return this.baseAttackDamage;
    }

    public int getMaxHealth() {
        return this.maxHealth;
    }

    public string getId() {
        return this.id;
    }
}
