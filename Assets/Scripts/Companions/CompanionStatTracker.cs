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

    public int numDeaths;

    public CompanionStatTracker() { }

    public CompanionStatTracker(List<Companion> toCombine)
    {
        // Sum over the num cards removed for now.
        this.numCardsRemoved = toCombine.Sum(c => c.trackingStats.numCardsRemoved);

        this.numDeaths = toCombine.Sum(c => c.trackingStats.numDeaths);

        this.cardsBought = toCombine.SelectMany(c => c.trackingStats.cardsBought).ToList();
    }

    public CompanionStatTracker(CompanionStatTrackerSerializable serializable, SORegistry registry)
    {
        this.numCardsRemoved = serializable.numCardsRemoved;
        this.numDeaths = serializable.numDeaths;
        this.cardsBought = serializable.cardsBought.Select(c => new Card(c, registry)).ToList();
    }

    public void RecordCardBuy(Card card)
    {
        cardsBought.Add(card);
    }

    public void RecordCardRemoval()
    {
        numCardsRemoved += 1;
    }

    public void RecordDeath()
    {
        numDeaths += 1;
    }
}

public class CompanionSellValue
{
    public int sellValueFromCompanions;
    public int sellValueFromCardsBought;
    public int sellValueFromCardsRemoved;

    public int sellValueReductionFromDeaths = 0;
    public int numDeaths = 0;

    public CompanionSellValue(ShopDataSO shopData, CompanionStatTracker stats, CompanionLevel level)
    {
        int numCompanions = 0;
        switch (level)
        {
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
        sellValueFromCompanions = (int) Math.Floor(numCompanions * shopData.numCompanionsSellFactor);
        sellValueFromCardsBought = (int) Math.Floor(stats.cardsBought.Count * shopData.numCardsBoughtSellFactor);
        sellValueFromCardsRemoved = (int) Math.Floor(stats.numCardsRemoved * shopData.numCardsRemovedSellFactor);
        if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.WORSE_RATES_FOR_REBORN_RATS))
        {
            var numDeathsSellFactor = (int)ProgressManager.Instance.GetAscensionSO(AscensionType.WORSE_RATES_FOR_REBORN_RATS).
                ascensionModificationValues.GetValueOrDefault("numDeathsSellFactor", 0f);
            sellValueReductionFromDeaths = stats.numDeaths * numDeathsSellFactor;
        }
        numDeaths = stats.numDeaths;
    }

    public int Total() {
        int total = this.sellValueFromCardsBought + this.sellValueFromCardsRemoved + this.sellValueFromCompanions - this.sellValueReductionFromDeaths;
        return Math.Max(total, 0);
    }
}

[System.Serializable]
public class CompanionStatTrackerSerializable
{
    public int numCardsRemoved;
    public List<CardSerializable> cardsBought;

    public int numDeaths;

    public CompanionStatTrackerSerializable(CompanionStatTracker stats)
    {
        this.numCardsRemoved = stats.numCardsRemoved;
        this.cardsBought = stats.cardsBought.Select(c => new CardSerializable(c)).ToList();
        this.numDeaths = stats.numDeaths;
    }
}
