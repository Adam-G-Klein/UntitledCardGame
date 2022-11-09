using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion
{
    public CompanionTypeSO companionType;
    public int currentHealth;
    public Deck deck;
    public string id;
    public int currentAttackDamage;

    public Companion(CompanionTypeSO companionType) 
    {
        this.companionType = companionType;
        this.currentHealth = companionType.maxHealth;
        this.currentAttackDamage = companionType.baseAttackDamage;
        this.deck = new Deck(companionType.startingDeck);
        this.id = Id.newGuid();
        Debug.Log(JsonUtility.ToJson(this));
    }
}
