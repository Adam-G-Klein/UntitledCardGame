using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StartinDeck", 
    menuName = "Decks/Starting Deck")]
public class StartingDeck : ScriptableObject
{
    public List<CardInfo> cards;
}
