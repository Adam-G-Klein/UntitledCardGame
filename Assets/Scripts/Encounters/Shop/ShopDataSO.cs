using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
public class ShopDataSO : ScriptableObject
{
    public List<ShopLevel> shopLevels;
    public CompanionPoolSO companionPool;
    public int keepsakeCount;
    public int rerollShopPrice;
    public int upgradeShopPrice;
    public ShopEncounterEvent shopEncounterEvent;

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
    public int manaIncrease;
    public int teamSize;
    public int upgradeCost;

    [Space(10)]
    [Header("Percentage Manipulation")]
    public int commonCompanionPercentage;
    public int uncommonCompanionPercentage;
    public int rareCompanionPercentage;
    public int commonCardPercentage;
    public int uncommonCardPercentage;
    public int rareCardPercentage;

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
