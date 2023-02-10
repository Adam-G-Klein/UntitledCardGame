using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Card: IEquatable<Card> 
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

    public static bool operator !=(Card a, Card b) {
        return !(a == b);
    }
    public static bool operator ==(Card a, Card b) {
        if (a is null && b is null) {
            return true;
        }
        if (a is null || b is null) {
            return false;
        }
        return a.id == b.id;
    }

    public override bool Equals(object other)
    {
        if(other is Card) {
            return ((Card) other) == this;
        }
        return false;
    }

    public bool Equals(Card other)
    {
        return other == this;
    }
    
    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
}
