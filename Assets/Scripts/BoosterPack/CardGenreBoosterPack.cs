using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardGenreBoosterPack : BoosterPack
{
    private PackSO packSO;
    private List<CompanionTypeSO> rats;

    public CardGenreBoosterPack(List<CompanionTypeSO> rats, BoosterPackType boosterType) {
        this.rats = rats.Select(rat => rat.GetBaseCompanionType()).ToList();
        if (rats.Count > 0) {
            this.packSO = rats[0].pack;
        }
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
        foreach (CompanionTypeSO rat in rats) {
            distBuilder = distBuilder.AddRatCardPool(rat, GetCardPoolForRat(rat));
        }
        distBuilder = distBuilder.AddPackCardPool(packSO, GetCardPoolForPack(packSO));

        ShopProbabilityDistribution dist = distBuilder.Build();

        // Print the distribution.
        Debug.Log("Generating cards for card genre booster pack: " + BoosterPackName());
        dist.PrintCardDistribution();

        List<CardInShopWithPrice> cards = dist.ChooseWithoutReplacement(data.numCards);
        cards.Shuffle();
        return cards;
    }

    private CardPoolSO GetCardPoolForRat(CompanionTypeSO rat) {
        return rat.cardPool;
    }

    private CardPoolSO GetCardPoolForPack(PackSO pack) {
        return pack.packCardPoolSO;
    }

    public override PackSO GetPackSO()
    {
        return packSO;
    }

    public override string BoosterPackName()
    {
        return packSO.packName + "\n" + GetBoosterPackTypeName(boosterType) + " Pack";
    }

    public override int GetBoosterPackPrice(ShopDataSO shopData)
    {
        BoosterPackData data = shopData.GetBoosterPackDataForType(boosterType);
        return data.price;
    }

    protected override string GetTooltipDescription()
    {
        List<string> ratNames = rats.Select(rat => rat.companionName).ToList();
        string ratNamesString = string.Join(", ", ratNames);
        return "Contains cards from rats " + ratNamesString + " and the " + packSO.packName + " pack.";
    }
}
