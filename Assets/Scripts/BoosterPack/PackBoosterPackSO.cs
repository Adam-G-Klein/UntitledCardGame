// filepath: /Users/jamesconant/UntitledCardGame/Assets/Scripts/Cards/BoosterPackSO.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PackBoosterPack : BoosterPack
{
    // Reference to the pack for which this booster pack belongs.
    public PackSO ratPackSO;

    public PackBoosterPack(PackSO pack, BoosterPack.BoosterPackType boosterType) {
        this.ratPackSO = pack;
        this.boosterType = boosterType;
    }
    public override List<CardInShopWithPrice> GenerateCards(
        ShopDataSO shopData,
        ShopLevel shopLevel
    ) {
        BoosterPackData pcts = shopData.GetBoosterPackDataForType(boosterType);
        ShopCardProbabilityDistBuilder probabilityDistBuilder = new ShopCardProbabilityDistBuilder(shopData, ratPackSO, pcts.commonCardPct, pcts.uncommonCardPct, pcts.rareCardPct, shopData.boosterPackCardPrice);
        ShopProbabilityDistribution dist = probabilityDistBuilder.Build();

        Debug.Log("Generating cards for pack booster pack: " + ratPackSO.packName);
        dist.PrintCardDistribution();
        return dist.ChooseWithoutReplacement(pcts.numCards);
    }

    public override PackSO GetPackSO()
    {
        return ratPackSO;
    }

    public override string BoosterPackName()
    {
        return ratPackSO.packName + " " + GetBoosterPackTypeName(boosterType) + " Pack";
    }

    public override int GetBoosterPackPrice(ShopDataSO shopData)
    {
        BoosterPackData data = shopData.GetBoosterPackDataForType(boosterType);
        return data.price;
    }
}
