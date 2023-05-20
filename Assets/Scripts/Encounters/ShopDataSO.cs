using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
public class ShopDataSO : ScriptableObject
{
    [Space(20)]
    [Header("Companions")]
    [Space(10)]
    [Header("Commons")]
    public int commonCompanionPercentage;
    public int commonCompanionPrice;
    public List<CompanionTypeSO> commonCompanions;

    [Header("Uncoommons")]
    public int uncommonCompanionPercentage;
    public int uncommonCompanionPrice;
    public List<CompanionTypeSO> uncommonCompanions;

    [Header("Rares")]
    public int rareCompanionPercentage;
    public int rareCompanionPrice;
    public List<CompanionTypeSO> rareCompanions;

    [Header("Misc")]
    public int keepsakeCount;
    public int rerollShopPrice;
    public int upgradeShopPrice;
    public ShopEncounterEvent shopEncounterEvent;

    public int getTotalCompanionPercentage() {
        return commonCompanionPercentage +
            uncommonCompanionPercentage +
            rareCompanionPercentage;
    }
}
