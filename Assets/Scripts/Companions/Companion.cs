using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Companion
{
    public CompanionType companionType;
    public int currentHealth;
    public Deck deck;
    public string id = Id.newGuid();

    public Companion(CompanionType companionType) 
    {
        this.companionType = companionType;
        this.currentHealth = companionType.maxHealth;
        this.deck = new Deck(companionType.startingDeck);
    }
}
