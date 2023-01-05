using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Card 
{
    
    [HideInInspector]
    public string name {
        get {
            return cardType.Name;
        }
    }

    [HideInInspector]
    public int cost {
        get {
            return cardType.Cost;
        }
    }

    [HideInInspector]
    public Sprite artwork {
        get {
            return cardType.Artwork;
        }
    }

    [HideInInspector]
    public List<EffectProcedure> effectProcedures {
        get {
            return cardType.EffectProcedures;
        }
    }

    public string description {
        get {
            return cardType.Description;
        }
    }

    [SerializeReference]
    public CardType cardType;
    public string id;

    public Card(CardType cardType)
    {
        this.cardType = cardType;
        id = Id.newGuid();
    }
}
