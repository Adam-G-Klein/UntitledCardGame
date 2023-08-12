using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Card : Entity, IEquatable<Card> 
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
    // IMPORTANT TODO: only effects cards that use effectIncreasesOnPlay right now, other things don't poll for this
    // Need to add this into the getEffectScale that's currently in the CasterStats right now
    private Dictionary<CombatEffect, int> effectBuffs = new Dictionary<CombatEffect, int>();

    [HideInInspector]
    public string description {
        get {
            String retDescription = cardType.Description;
            if (effectBuffs == null || effectBuffs.Count == 0) {
                return retDescription;
            }
            foreach(CombatEffect effect in effectBuffs.Keys){
                if(effectBuffs[effect] != 0) {
                    retDescription += "\n" + "(" + effectBuffs[effect] + " additional " + effect.ToString() + ")";
                }
            }
            return retDescription;
        }
    }


    [SerializeReference]
    public CardType cardType;

    public Card(CardType cardType)
    {
        this.cardType = cardType;
        this.id = Id.newGuid();
    }

    public Card(Card card) {
        this.cardType = card.cardType;
        id = card.id;
        this.effectBuffs = card.effectBuffs;
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

    public int GetEffectBuff(CombatEffect effect) {
        // static constructor not working for the dict, don't know why 
        if(effectBuffs == null) {
            effectBuffs = new Dictionary<CombatEffect, int>();
        }
        if (effectBuffs.ContainsKey(effect)) {
            return effectBuffs[effect];
        }
        return 0;
    }

    public void BuffEffect(CombatEffect effect, int buff) {
        // static constructor not working for the dict, don't know why 
        if(effectBuffs == null) {
            effectBuffs = new Dictionary<CombatEffect, int>();
        }
        if (effectBuffs.ContainsKey(effect)) {
            effectBuffs[effect] += buff;
        } else {
            effectBuffs.Add(effect, buff);
        }
    }
}
