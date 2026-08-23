using System.Collections.Generic;
using UnityEngine;

public abstract class BoosterPack : ITooltipProvider
{
    public enum BoosterPackType
    {
        CommonRat,
        UncommonRat,
        // Card genre booster packs
        CardGenreOffense,
        CardGenreDefense,
        CardGenreUtility,
        // Pack booster packs
        PackBasicBooster,
        PackDeluxeBooster,
        UpgradedPackBasicBooster,
        UpgradedPackDeluxeBooster,
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
                return "Common";
            case BoosterPackType.UncommonRat:
                return "Rare";
            case BoosterPackType.CardGenreOffense:
                return "Offense";
            case BoosterPackType.CardGenreDefense:
                return "Defense";
            case BoosterPackType.CardGenreUtility:
                return "Utility";
            case BoosterPackType.PackBasicBooster:
            case BoosterPackType.UpgradedPackBasicBooster:
                return "Basic";
            case BoosterPackType.PackDeluxeBooster:
            case BoosterPackType.UpgradedPackDeluxeBooster:
                return "Premium";
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
                return "Cheap essentials - mix of common & uncommon cards";
            case BoosterPackType.UncommonRat:
                return "Get deeper into a strategy - this pack contains powerful uncommon and rare cards.";
            case BoosterPackType.CardGenreOffense:
                return "Offensive cards for your strategy.";
            case BoosterPackType.CardGenreDefense:
                return "Defensive cards for your strategy.";
            case BoosterPackType.CardGenreUtility:
                return "Utility cards for your strategy.";
            case BoosterPackType.PackBasicBooster:
            case BoosterPackType.UpgradedPackBasicBooster:
                return "Cheap essentials - choose from a mix of common and uncommon cards.";
            case BoosterPackType.PackDeluxeBooster:
            case BoosterPackType.UpgradedPackDeluxeBooster:
                return "Fancy things - choose from a mostly uncommon and rare cards.";
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
