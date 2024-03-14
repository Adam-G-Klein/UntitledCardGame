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

    [HideInInspector]
    private CompanionTypeSO companionFrom;

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

    public Card(CardType cardType, CompanionTypeSO companionFrom)
    {
        this.cardType = cardType;
        this.id = Id.newGuid();
        this.setCompanionFrom(companionFrom);
        ResetCardModifications();
    }

    public Card(Card card) {
        this.cardType = card.cardType;
        id = card.id;
        this.effectBuffs = card.effectBuffs;
        this.setCompanionFrom(card.getCompanionFrom());
        ResetCardModifications();
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
        int totalReduction = 0;
        Debug.Log("CardModifications: " + cardModifications);
        if(cardModifications == null) {
            ResetCardModifications();
        }
        totalReduction += cardModifications[CardModification.TempManaDecrease];
        totalReduction += cardType.cardModifications[CardModification.TempManaDecrease];
        
        return Mathf.Max(0, cardType.Cost - totalReduction);
    }

    public int UpdateScaleForCardModifications(int oldScale) {
        int newScale = oldScale;
        // Making the choice to do fixed damage first, then multiplication. I don't think it will matter unless
        // both effects end up on one single card
        newScale += cardModifications[CardModification.FixedDamageIncrease];
        newScale += cardType.cardModifications[CardModification.FixedDamageIncrease];
        newScale = newScale * (int)(Mathf.Pow(2, cardModifications[CardModification.DoubleDamageIncrease]));
        newScale = newScale * (int)(Mathf.Pow(2, cardType.cardModifications[CardModification.DoubleDamageIncrease]));

        return newScale;
    }

    public void ResetTempCardModifications() {
        cardModifications[CardModification.TempManaDecrease] = 0;
    }

    public void ResetCardModifications() {
        cardModifications = new Dictionary<CardModification, int>();
        foreach(int i in Enum.GetValues(typeof(CardModification))) {
            cardModifications.Add((CardModification)i, 0);
        }
        cardType.ResetCardModifications();
    }

    public void ChangeCardModification(CardModification modification, int scale) {
        cardModifications[modification] += scale;
    }

    public void setCompanionFrom(CompanionTypeSO companion) {
        companionFrom = companion;
    }
    
    public CompanionTypeSO getCompanionFrom() {
        return companionFrom;
    }
}
