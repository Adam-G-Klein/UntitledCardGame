using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
[CreateAssetMenu(
    fileName = "Companion",
    menuName = "Companions/TestCompanion")]
public class Companion: ScriptableObject
{
    public CompanionType companionType;
    public int currentHealth;
    public Deck deck;
    public string id = Id.newGuid();

    public Companion(CompanionType companionType) 
    {
        this.currentHealth = companionType.maxHealth;
        this.deck = new Deck(companionType.startingDeck);
    }
}
