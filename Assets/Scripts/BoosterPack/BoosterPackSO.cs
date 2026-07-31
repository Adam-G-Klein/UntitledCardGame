using System.Collections.Generic;
using UnityEngine;

public abstract class BoosterPack
{
    public enum BoosterPackType
    {
        CommonRat,
        UncommonRat,
        CommonPack,
        UncommonPack,
    }
    public BoosterPackType boosterType;

    public abstract List<CardInShopWithPrice> GenerateCards(
        ShopDataSO shopData,
        ShopLevel shopLevel
    );

    public abstract PackSO GetPackSO();

    public abstract string BoosterPackName();

    public abstract int GetBoosterPackPrice(ShopDataSO shopData);

    protected static string GetBoosterPackTypeName(BoosterPackType boosterType)
    {
        switch (boosterType)
        {
            case BoosterPackType.CommonRat:
            case BoosterPackType.CommonPack:
                return "Common";
            case BoosterPackType.UncommonRat:
            case BoosterPackType.UncommonPack:
                return "Uncommon";
            default:
                return "Unknown Booster Pack Type";
        }
    }
}
