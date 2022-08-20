using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect_Draw: CardEffect
{
    public int Scale = 1;
    public override void Execute()
    {
        //TODO
        //call effect manager to draw Scale amount of cards
        Debug.Log("DRAWING " + Scale + " CARDS"); 
    }
}
