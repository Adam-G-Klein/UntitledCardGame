using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffect_Damage
{
    public static void Execute(CardEffectData card, CardCastArguments args)
    {
        // Alright before I delete all of these,
        // I'm pretty sure that we don't need these anymore
        // They were here because we figured that each effect might have 
        // a bunch of different coded things to do
        // But now I figure each of those effects will just listen for the 
        // effect type to be raised? Feel free to delete this if you agree
        Debug.Log("Dealing " + card.scale + " damage to targets " + args.targets); 
    }
}
