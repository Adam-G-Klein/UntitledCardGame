using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Companion
{
    public CompanionType companionType;
    public int currentHealth;
    public Deck deck;

    public Companion(CompanionType companionType) 
    {
        this.currentHealth = companionType.maxHealth;
        this.deck = new Deck(companionType.startingDeck);
    }
}
