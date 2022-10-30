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
    // Okay Luke I'm conflicted.
    // Setting this in the editor every time we make a card will be a pain
    // But this looks so much worse and I think it's the only alternative?
    // https://docs.unity3d.com/ScriptReference/Resources.html
    // See Cast() for why this is a problem
    [SerializeField]
    private CardEffectEvent cardEffectEvent;

    //called when cast to trigger series of effects on this card
    public void Cast(CardCastArguments args) {
        Debug.Log("Casting " + Name + " with target: " + (args.targets.Count > 0 ? args.targets[0] : "null"));
        for(int i = 0; i < EffectsList.Count; i++) {
            // like, we need to raise the event, but I think the only way to get it here
            // is with a reference on the class unless we use Resources.Load 
            // plz lmk if you have a better idea :/
            // WAIT I BET ITS A CUSTOM EDITOR
            // I don't know if I'll do that as a part of this commit just comment "yep" 
            // on this line of code if I'm right

            cardEffectEvent.Raise(
                new CardEffectEventInfo(EffectsList[i].effectName, EffectsList[i].scale, args.targets));

        }
    }
}
