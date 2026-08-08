using System.Collections.Generic;
using UnityEngine;

public abstract class BoosterPack : ITooltipProvider
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
                return "Rare";
            default:
                return "Unknown Booster Pack Type";
        }
    }

    protected abstract string GetTooltipDescription();

    private string BoosterPackTypeTooltipDescription()
    {
        switch (boosterType)
        {
            case BoosterPackType.CommonRat:
            case BoosterPackType.CommonPack:
                return "Cheap essentials - this pack contains common and some uncommon cards.";
            case BoosterPackType.UncommonPack:
            case BoosterPackType.UncommonRat:
                return "Get deeper into a strategy - this pack contains powerful uncommon and rare cards.";
            default:
                return "Unknown booster pack type.";
        }
    }

    public TooltipViewModel GetTooltip()
    {
        string title = BoosterPackName();
        string description = GetTooltipDescription();

        description += "\n\n" + BoosterPackTypeTooltipDescription();

        return new TooltipViewModel(title, description);
    }
}
