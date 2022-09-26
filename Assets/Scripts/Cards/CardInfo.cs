using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Cards/New Card")]
public class CardInfo : ScriptableObject
{
    public string Name;
    public string Description;
    public int Cost;
    public Sprite Artwork;
    public List<CardEffectData> EffectsList;
    public string id = Id.newGuid();

    //called when cast to trigger series of effects on this card
    public void Cast(CardCastArguments args) {
        Debug.Log("Casting " + Name + "!");
        for(int i = 0; i < EffectsList.Count; i++) {
            CardExecutor.executeCard(EffectsList[i], args);
        }
    }
}
