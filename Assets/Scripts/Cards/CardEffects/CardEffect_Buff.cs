using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffect_Buff
{
    public static void Execute(CardEffectData card, CardCastArguments args)
    {
        //TODO
        //call effect manager to draw Scale amount of cards
        Debug.Log("Buffing target " + args.target + " by " + card.scale); 
    }
}
