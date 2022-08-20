using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Cards/New Card")]
public class CardInfo : ScriptableObject
{
    public string Name;
    public string Description;
    public int Cost;
    public Sprite Artwork;
    public List<CardEffect> Effects;

    //called when cast to trigger series of effects on this card
    public void Cast() {
        Debug.Log("Casting " + Name + "!");
        foreach (var Eff in Effects) {
            Eff.Execute();
        }
    }
}
