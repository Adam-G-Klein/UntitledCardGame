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
            return cardType.GetName();
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
            if(cardType.effectWorkflows == null || cardType.effectWorkflows.Count == 0 || cardType.effectWorkflows.Count < workflowIndex) {
                Debug.LogError("Attempted to get workflow " + workflowIndex + " of card " +
                name + ", but it only has " + cardType.effectWorkflows.Count + " workflows");
                return null;
            }
            return cardType.effectWorkflows[workflowIndex].effectSteps;
        }

    }

    [HideInInspector]
    private CompanionTypeSO companionFrom;

    public CardRarity shopRarity = CardRarity.NONE;

    // For sagas, determines the index into the EffectWorkflowList that we'll return from GetEffectWorkflow
    // For non-sagas, will always stay at 0
    private int workflowIndex = 0;
    public int castCount = 0;
    public int tempManaCost = -1;

    // Generated cards
    public bool generated = false;
    // IMPORTANT TODO: only effects cards that use effectIncreasesOnPlay right now, other things don't poll for this
    // Need to add this into the getEffectScale that's currently in the CasterStats right now
    // Deprecated
    private Dictionary<CombatEffect, int> effectBuffs = new Dictionary<CombatEffect, int>();
    public Dictionary<CardModification, int> cardModifications = new Dictionary<CardModification, int>();

    [SerializeReference]
    public CardType cardType;

    public Card(CardType cardType, CompanionTypeSO companionFrom, CardRarity rarity = CardRarity.NONE)
    {
        this.cardType = cardType;
        this.shopRarity = rarity;
        this.id = Id.newGuid();
        this.setCompanionFrom(companionFrom);
        ResetCardModifications();
    }

    public Card(Card card) {
        this.shopRarity = card.shopRarity;
        this.cardType = card.cardType;
        id = card.id;
        this.effectBuffs = card.effectBuffs;
        this.generated = card.generated;
        this.setCompanionFrom(card.getCompanionFrom());
        ResetCardModifications();
    }

    public Card(CardSerializable cardSerializable, SORegistry registry)
    {
        this.cardType = registry.GetAsset<CardType>(cardSerializable.cardTypeGuid);
        this.id = cardSerializable.entityId;
        this.generated = cardSerializable.generated;
        this.setCompanionFrom(registry.GetAsset<CompanionTypeSO>(cardSerializable.companionFromGuid));
        this.shopRarity = cardSerializable.cardRarity;
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

    public int GetManaCost()
    {
        int totalReduction = 0;
        if (cardModifications == null)
        {
            ResetCardModifications();
        }
        totalReduction += cardModifications[CardModification.UntilLeavesHandManaDecrease];
        totalReduction += cardType.cardModifications[CardModification.UntilLeavesHandManaDecrease];
        totalReduction += cardModifications[CardModification.ThisCombatManaDecrease];
        totalReduction += cardType.cardModifications[CardModification.ThisCombatManaDecrease];

        return Mathf.Max(0, cardType.Cost - totalReduction);
    }

    public Dictionary<string, int> GetDefaultValuesMap(CombatInstance origin)
    {
        Dictionary<string, int> intMap = new();
        foreach (var defaultValue in cardType.defaultValues)
        {
            int value = defaultValue.value;
            // If we are in the shop, the combat instance will be null.
            if (origin != null)
            {
                value = UpdateScaleForCardModificationsAndPassives(defaultValue.value, origin);
            }
            intMap[defaultValue.key] = value;
        }
        return intMap;
    }

    public int UpdateScaleForCardModificationsAndPassives(int oldScale, CombatInstance origin) {
        int newScale = oldScale;
        // FollowUp: If we cast an attack last, our next attack deals +3 damage.
        if (origin.HasPower(PowerSO.PowerType.FollowUp))
        {
            Card lastCastCard = EnemyEncounterManager.Instance.combatEncounterState.GetLastCastCard();
            // Debug.Log("Last cast card category: " + lastCastCard.cardType.cardCategory + ", num stack of power")
            if (lastCastCard != null && lastCastCard.cardType.cardCategory == CardCategory.Attack)
            {
                newScale += 3 * origin.GetNumStackOfPower(PowerSO.PowerType.FollowUp);
            }
        }
        // RainOfBlows: each attack deals +1 damage for each attack played this turn.
        if (origin.HasPower(PowerSO.PowerType.RainOfBlows))
        {
            int numAttacks = EnemyEncounterManager.Instance.combatEncounterState.GetNumCardsOfCategoryPlayedThisTurn(CardCategory.Attack);
            newScale += numAttacks * origin.GetNumStackOfPower(PowerSO.PowerType.RainOfBlows);
        }
        // HotStreak: deal +1 damage for each card discarded this turn
        if (origin.HasPower(PowerSO.PowerType.HotStreak))
        {
            int numDiscarded = EnemyEncounterManager.Instance.combatEncounterState.cardsDiscardedThisTurn.Count;
            newScale += numDiscarded * origin.GetNumStackOfPower(PowerSO.PowerType.HotStreak);
        }

        // Making the choice to do fixed damage first, then multiplication. I don't think it will matter unless
        // both effects end up on one single card
        newScale += cardModifications[CardModification.FixedDamageIncrease];
        newScale += cardType.cardModifications[CardModification.FixedDamageIncrease];
        newScale *= (int) Mathf.Pow(2, cardModifications[CardModification.DoubleDamageIncrease]);
        newScale *= (int) Mathf.Pow(2, cardType.cardModifications[CardModification.DoubleDamageIncrease]);

        // DoubleDamage: If we cast an attack last, our next attack deals +3 damage.
        if (origin.HasPower(PowerSO.PowerType.DoubleDamage))
        {
            newScale *= (int) Math.Pow(2, origin.GetNumStackOfPower(PowerSO.PowerType.DoubleDamage));
        }

        return newScale;
    }


    public void ResetCardModifications() {
        cardModifications = new Dictionary<CardModification, int>();
        foreach(int i in Enum.GetValues(typeof(CardModification))) {
            cardModifications.Add((CardModification)i, 0);
        }
        // cardType.ResetCardModifications();
    }

    public void ChangeCardModification(CardModification modification, int scale) {
        Debug.Log("Changing card id " + id + ": " + modification + " by " + scale);
        cardModifications[modification] += scale;
    }

    public void setCompanionFrom(CompanionTypeSO companion) {
        companionFrom = companion;
    }

    public CompanionTypeSO getCompanionFrom() {
        return companionFrom;
    }

    public bool CardModificationsHasKey(CardModification mod) {
        return cardModifications.ContainsKey(mod) && cardModifications[mod] != 0;
    }

    public enum CardRarity {
        NONE,
        COMMON,
        UNCOMMON,
        RARE,
    }
}

[System.Serializable]
public class CardSerializable
{
    public string cardTypeGuid;
    public string entityId;
    public string companionFromGuid;
    public bool generated;
    public Card.CardRarity cardRarity;
    public CardSerializable(Card card)
    {
        // TODO
        this.cardTypeGuid = card.cardType.GUID;
        this.entityId = card.id;
        this.companionFromGuid = card.getCompanionFrom().GUID;
        this.generated = card.generated;
        this.cardRarity = card.shopRarity;
    }
}
