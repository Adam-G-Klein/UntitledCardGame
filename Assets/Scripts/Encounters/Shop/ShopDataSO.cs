using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopMode
{
    Default,
    StaticChooseNDemo,
    BoosterPacks,
}

[Serializable]
public class BoosterPackData
{
    public BoosterPack.BoosterPackType boosterType;
    public int commonCardPct;
    public int uncommonCardPct;
    public int rareCardPct;
    public int price;
    public int numCards = 4;
    public int ratBoosterPackNumCardsFromRatMin = 1;
    public int ratBoosterPackNumCardsFromRatMax = 2;
}

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
[System.Serializable]
public class ShopDataSO : IdentifiableSO
{
    public List<CrossPackCardPoolSO> activeCrossPackCardPools;

    public int rerollShopPrice;
    public float incrementalRerollPriceIncrease = 1f;
    public int cardRemovalPrice;
    public int cardRemovalPriceIncrease = 4;
    public int switchDecksPrice = 2;

    public int commonCardPrice = 1;
    public int uncommonCardPrice = 2;
    public int rareCardPrice = 3;

    public int companionKeepsakePrice;

    // The number of keepsake copies avaialble in the shop.
    // There is a finite amount of each companion available, in order
    // to force the player to try other strategies.
    public int numKeepsakeCopies;

    [Header("Shop probabilities for card source (should add up to 100)")]
    public int companionTypeCardPoolPct;
    public int packCardPoolPct;
    public int neutralCardPoolPct;
    public int crossPackCardPoolPct;
    public bool alwaysShowUnlockedCards = false;

    public ShopMode shopMode;

    [Header("Demo only fields")]
    public int numCardsBuyPerShop = 5;
    public int numCardsBuyPerDisplay = 1;
    public int numRatsBuyPerShop = 6;
    public int numRatsBuyPerDisplay = 2;
    public int freeMoney = 60;
    public int freeRerolls = 6;
    public int freeCardRemovals = 2;
    public int maximumMoneyLeftBeforeNextCombat = 10;

    [Header("Neutral cards")]
    public CardPoolSO neutralCardPool;

    [Header("Selling the companions in the shop")]
    public float numCompanionsSellFactor = 1.5f;
    public float numCardsBoughtSellFactor = 0.5f;
    public float numCardsRemovedSellFactor = 4f;

    [Header("Booster pack data")]
    public int boosterPackCardPrice = 0;
    public int boosterPackNumCardsToBuy = 2;
    public List<BoosterPackData> boosterPackPcts;

    [Header("Upgrade parameters")]
    public List<CardType> baseCardsToRemoveOnUpgrade;

    public BoosterPackData GetBoosterPackDataForType(BoosterPack.BoosterPackType boosterType) {
        foreach (BoosterPackData p in boosterPackPcts) {
            if (p.boosterType == boosterType) {
                return p;
            }
        }
        throw new Exception("No booster pack data found for rarity: " + boosterType);
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
    public int numBoosterPacksToShow;

    [Space(10)]
    [Header("Percentage Manipulation")]
    public int commonCompanionPercentage;
    public int uncommonCompanionPercentage;
    public int rareCompanionPercentage;
    public int commonCardPercentage;
    public int uncommonCardPercentage;
    public int rareCardPercentage;
    [Serializable]
    public class BoosterPackPcts
    {
        public BoosterPack.BoosterPackType boosterType;
        public int appearancePct;
    }
    public List<BoosterPackPcts> boosterPackPcts;

    [Space(10)]
    [Header("Unique bonuses")]
    public int ratBonusHealth;
    public int cardRemovalDiscount;
    public int numCardRemovalsAllowed = 1;
    public int numLessCardsInStartingDeck;
    public bool switchDecksEnabled = true;
    public bool cardRemovalEnabled = true;

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

    public int GetBoosterPackAppearancePct(BoosterPack.BoosterPackType boosterType) {
        foreach (BoosterPackPcts p in boosterPackPcts) {
            if (p.boosterType == boosterType) {
                return p.appearancePct;
            }
        }
        return 0;
    }
}
