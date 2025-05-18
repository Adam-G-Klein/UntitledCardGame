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
    public CompanionPoolSO companionPool;
    public int rerollShopPrice;
    public int cardRemovalPrice;
    public ShopEncounterEvent shopEncounterEvent;

    public int cardPrice;
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

    [Header("Earning post combat")]
    public int goldEarnedPerBattle;
    public float interestRate;
    public int interestCap;

    public CardPoolSO neutralCardPool;

    [Header("Selling the companions in the shop")]
    public int levelOneSellPrice;
    public int levelTwoSellPrice;
    public int levelThreeSellPrice;
    public float deckSizeSellFactor;

    public float numCompanionsSellFactor = 1.5f;
    public float numCardsBoughtSellFactor = 0.5f;
    public float numCardsRemovedSellFactor = 4f;

    [Header("Upgrade parameters")]
    public List<CardType> baseCardsToRemoveOnUpgrade;

    public ShopLevel GetShopLevel(int level) {
        foreach (ShopLevel shopLevel in shopLevels) {
            if (shopLevel.level == level) {
                return shopLevel;
            }
        }
        // Default to whatever is first in the list
        Debug.LogWarning("Specified shop level not found, using default shop level");
        return shopLevels[0];
    }
}

[Serializable]
public class ShopLevel {
    [Header("Basic Data")]
    public int level;
    public int shopLevelIncrementsToUnlock;
    public int upgradeIncrementCost;
    public int mana;
    public int teamSize;
    public int upgradeCost;
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
