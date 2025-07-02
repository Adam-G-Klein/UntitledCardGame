using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

[System.Serializable]
public class CompanionStatTracker
{
    public int numCardsRemoved;
    public List<Card> cardsBought = new();

    public CompanionStatTracker() {}

    public CompanionStatTracker(List<Companion> toCombine) {
        // Sum over the num cards removed for now.
        this.numCardsRemoved = toCombine.Sum(c => c.trackingStats.numCardsRemoved);

        this.cardsBought = toCombine.SelectMany(c => c.trackingStats.cardsBought).ToList();
    }

    public CompanionStatTracker(CompanionStatTrackerSerializable serializable, SORegistry registry) {
        this.numCardsRemoved = serializable.numCardsRemoved;
        this.cardsBought = serializable.cardsBought.Select(c => new Card(c, registry)).ToList();
    }

    public void RecordCardBuy(Card card) {
        cardsBought.Add(card);
    }

    public void RecordCardRemoval() {
        numCardsRemoved += 1;
    }
}

public class CompanionSellValue
{
    public int sellValueFromCompanions;
    public int sellValueFromCardsBought;
    public int sellValueFromCardsRemoved;

    public CompanionSellValue(ShopDataSO shopData, CompanionStatTracker stats, CompanionLevel level) {
        int numCompanions = 0;
        switch (level) {
        case CompanionLevel.LevelOne:
            numCompanions = 1;
            break;
        case CompanionLevel.LevelTwo:
            numCompanions = 3;
            break;
        case CompanionLevel.LevelThree:
            numCompanions = 6;
            break;
        }
        sellValueFromCompanions = Convert.ToInt32(numCompanions * shopData.numCompanionsSellFactor);
        sellValueFromCardsBought = Convert.ToInt32(stats.cardsBought.Count * shopData.numCardsBoughtSellFactor);
        sellValueFromCardsRemoved = Convert.ToInt32(stats.numCardsRemoved * shopData.numCardsRemovedSellFactor);
    }

    public int Total() {
        return this.sellValueFromCardsBought + this.sellValueFromCardsRemoved + this.sellValueFromCompanions;
    }
}

[System.Serializable]
public class CompanionStatTrackerSerializable
{
    public int numCardsRemoved;
    public List<CardSerializable> cardsBought;

    public CompanionStatTrackerSerializable(CompanionStatTracker stats) {
        this.numCardsRemoved = stats.numCardsRemoved;
        this.cardsBought = stats.cardsBought.Select(c => new CardSerializable(c)).ToList();
    }
}
