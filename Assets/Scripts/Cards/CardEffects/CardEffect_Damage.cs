using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffect_Damage
{
    public static void Execute(CardEffectData card, CardCastArguments args)
    {
        Debug.Log("Dealing " + card.scale + " damage to target " + args.target); 
    }
}
