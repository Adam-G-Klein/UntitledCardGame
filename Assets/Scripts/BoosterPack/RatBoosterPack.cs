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
        ShopCardProbabilityDistBuilder distBuilder = new ShopCardProbabilityDistBuilder(
            shopData,
            data.commonCardPct,
            data.uncommonCardPct,
            data.rareCardPct,
            data.ratSourcePct,
            data.packSourcePct,
            shopData.boosterPackCardPrice
        );

        distBuilder = distBuilder.AddRatCardPool(ratSO, ratSO.cardPool);
        distBuilder = distBuilder.AddPackCardPool(ratSO.pack, ratSO.pack.packCardPoolSO);

        ShopProbabilityDistribution dist = distBuilder.Build();

        List<CardInShopWithPrice> ret = dist.ChooseWithoutReplacement(data.numCards);
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
