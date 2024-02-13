using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

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

    [HideInInspector]
    public List<EffectStep> effectSteps {
        get {
            if(cardType.effectWorkflows.Count < workflowIndex) {
                Debug.LogError("Attempted to get workflow " + workflowIndex + " of card " +
                name + ", but it only has " + (cardType.effectWorkflows.Count + 1) + " workflows");
            }
            return cardType.effectWorkflows[workflowIndex].effectSteps;
        }

    }

    // For sagas, determines the index into the EffectWorkflowList that we'll return from GetEffectWorkflow
    // For non-sagas, will always stay at 0
    private int workflowIndex = 0;
    public int castCount = 0;
    // IMPORTANT TODO: only effects cards that use effectIncreasesOnPlay right now, other things don't poll for this
    // Need to add this into the getEffectScale that's currently in the CasterStats right now
    // Deprecated
    private Dictionary<CombatEffect, int> effectBuffs = new Dictionary<CombatEffect, int>();
    private Dictionary<CardModification, int> cardModifications = new Dictionary<CardModification, int>();

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
        InitializeCardModifications();
    }

    public Card(Card card) {
        this.cardType = card.cardType;
        id = card.id;
        this.effectBuffs = card.effectBuffs;
        InitializeCardModifications();
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

    public int GetWorkflowIndex() {
        return workflowIndex;
    }

    public void SetWorkflowIndex(int newIndex){
        workflowIndex = Mathf.Min(newIndex, cardType.effectWorkflows.Count - 1);
    }

    public int GetManaCost() {
        return cardType.Cost - cardModifications[CardModification.TempManaDecrease];
    }

    public int UpdateScaleForCardModifications(int oldScale) {
        int newScale = oldScale;
        if (cardModifications.ContainsKey(CardModification.FixedDamageIncrease)) {
            newScale += cardModifications[CardModification.FixedDamageIncrease];
        }

        if (cardModifications.ContainsKey(CardModification.DoubleDamageIncrease)) {
            newScale = newScale * (int)(Mathf.Pow(2, cardModifications[CardModification.FixedDamageIncrease]));
        }

        return newScale;
    }

    public void ResetTempCardModifications() {
        if (cardModifications[CardModification.TempManaDecrease] != 0) {
            cardModifications[CardModification.TempManaDecrease] = 0;
        }
    }

    public void InitializeCardModifications() {
        cardModifications = new Dictionary<CardModification, int>();
        foreach(int i in Enum.GetValues(typeof(CardModification))) {
            cardModifications.Add((CardModification)i, 0);
        }
    }
}
