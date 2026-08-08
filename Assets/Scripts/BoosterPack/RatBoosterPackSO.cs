// filepath: /Users/jamesconant/UntitledCardGame/Assets/Scripts/Cards/BoosterPackSO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RatBoosterPackSO : BoosterPack
{
    // Reference to the rat for which this booster pack belongs.
    public CompanionTypeSO ratSO;

    public RatBoosterPackSO(CompanionTypeSO ratSO, BoosterPack.BoosterPackType boosterType) {
        this.ratSO = ratSO;
        this.boosterType = boosterType;
    }

    public override List<CardInShopWithPrice> GenerateCards(
        ShopDataSO shopData,
        ShopLevel shopLevel
    ) {
        BoosterPackData data = shopData.GetBoosterPackDataForType(boosterType);

        // Build a probability distribution for the cards in the rat's card pool.
        ShopCardProbabilityDistBuilder ratProbabilityDistBuilder = new ShopCardProbabilityDistBuilder(shopData, ratSO, data.commonCardPct, data.uncommonCardPct, data.rareCardPct, shopData.boosterPackCardPrice);
        ShopProbabilityDistribution ratDist = ratProbabilityDistBuilder.Build();

        ShopCardProbabilityDistBuilder packProbabilityDistBuilder = new ShopCardProbabilityDistBuilder(shopData, ratSO.pack, data.commonCardPct, data.uncommonCardPct, data.rareCardPct, shopData.boosterPackCardPrice);
        ShopProbabilityDistribution packDist = packProbabilityDistBuilder.Build();

        // Print the rat dist.
        Debug.Log("Generating cards for rat booster pack: " + ratSO.companionName);
        ratDist.PrintCardDistribution();
        packDist.PrintCardDistribution();

        int numCards = UnityEngine.Random.Range(data.ratBoosterPackNumCardsFromRatMin, data.ratBoosterPackNumCardsFromRatMax + 1);
        int numCardsFromRat = Math.Min(numCards, data.numCards);

        List<CardInShopWithPrice> cardsFromRat = ratDist.ChooseWithoutReplacement(numCardsFromRat);
        List<CardInShopWithPrice> cardsFromPack = packDist.ChooseWithoutReplacement(Math.Max(0, data.numCards - numCardsFromRat));

        List<CardInShopWithPrice> ret = cardsFromRat.Concat(cardsFromPack).ToList();
        ret.Shuffle();
        return ret;
    }

    public override PackSO GetPackSO()
    {
        return ratSO.pack;
    }

    public override string BoosterPackName()
    {
        return ratSO.companionName + "\n" + GetBoosterPackTypeName(boosterType) + " Pack";
    }

    public override int GetBoosterPackPrice(ShopDataSO shopData)
    {
        BoosterPackData data = shopData.GetBoosterPackDataForType(boosterType);
        return data.price;
    }

    protected override string GetTooltipDescription()
    {
        return "Contains cards from the rat " + ratSO.companionName + "'s card pool and the " + ratSO.pack.packName + " card pool.";
    }
}
