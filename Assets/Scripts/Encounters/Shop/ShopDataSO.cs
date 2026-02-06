using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
[System.Serializable]
public class ShopDataSO : ScriptableObject
{
    public List<ShopLevel> shopLevels;
    // The companionPool is a consolidation of the companion pools across the activePacks.
    // It lies here for convenience's sake because we do not stratify shop offerings by packs for now.
    public CompanionPoolSO companionPool;
    public List<PackSO> activePacks;
    public List<CrossPackCardPoolSO> activeCrossPackCardPools;

    public int rerollShopPrice;
    public float incrementalRerollPriceIncrease = 1f;
    public int cardRemovalPrice;
    public int cardRemovalPriceIncrease = 4;
    public ShopEncounterEvent shopEncounterEvent;

    public int commonCardPrice = 1;
    public int uncommonCardPrice = 2;
    public int rareCardPrice = 3;

    public int companionKeepsakePrice;

    // The number of keepsake copies avaialble in the shop.
    // There is a finite amount of each companion available, in order
    // to force the player to try other strategies.
    public int numKeepsakeCopies;

    public int startingGold;
    [Header("Shop probabilities for card source (should add up to 100)")]
    public int companionTypeCardPoolPct;
    public int packCardPoolPct;
    public int neutralCardPoolPct;
    public int crossPackCardPoolPct;
    public bool alwaysShowUnlockedCards = false;

    [Header("Earning post combat")]
    public int goldEarnedPerBattle;
    public float interestRate;
    public int interestCap;
    [Header("Neutral cards")]

    public CardPoolSO neutralCardPool;

    [Header("Selling the companions in the shop")]

    public float numCompanionsSellFactor = 1.5f;
    public float numCardsBoughtSellFactor = 0.5f;
    public float numCardsRemovedSellFactor = 4f;

    [Header("Upgrade parameters")]
    public List<CardType> baseCardsToRemoveOnUpgrade;

    [Header("Controlling healing in the game")]
    public int benchHealingAmount;
    public int postEliteHealingAmount;

    public ShopLevel GetShopLevel(int level)
    {
        foreach (ShopLevel shopLevel in shopLevels)
        {
            if (shopLevel.level == level)
            {
                return shopLevel;
            }
        }
        // Default to whatever is first in the list
        Debug.LogWarning("Specified shop level not found, using default shop level");
        return shopLevels[0];
    }

    public int GetCardRemovalPrice(int level, int timesCardRemovedThisShop)
    {
        ShopLevel shopLevel = GetShopLevel(level);
        int cardRemovalPrice = this.cardRemovalPrice - shopLevel.cardRemovalDiscount;
        cardRemovalPrice += timesCardRemovedThisShop * cardRemovalPriceIncrease; // Each subsequent removal in the same shop costs more
        return Math.Max(0, cardRemovalPrice);
    }
}

[Serializable]
public class ShopLevel {
    [Header("Basic Data")]
    public int level;
    public int shopLevelIncrementsToUnlock;
    public int upgradeIncrementCost;
    public int numCardsToShow;
    public int numKeepsakesToShow;

    [Space(10)]
    [Header("Percentage Manipulation")]
    public int commonCompanionPercentage;
    public int uncommonCompanionPercentage;
    public int rareCompanionPercentage;
    public int commonCardPercentage;
    public int uncommonCardPercentage;
    public int rareCardPercentage;

    [Space(10)]
    [Header("Unique bonuses")]
    public int ratBonusHealth;
    public int cardRemovalDiscount;
    public int numCardRemovalsAllowed = 1;
    public int numLessCardsInStartingDeck;

    [Space(10)]
    [Header("Help the player understand this shit")]
    public TooltipViewModel upgradeTooltip;

    public int GetTotalCompanionPercentage() {
        return commonCompanionPercentage +
            uncommonCompanionPercentage +
            rareCompanionPercentage;
    }

    public int GetTotalCardPercentage() {
        return commonCardPercentage +
            uncommonCardPercentage +
            rareCardPercentage;
    }
}
