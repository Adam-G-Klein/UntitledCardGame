using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
public class ShopDataSO : ScriptableObject
{
    [Header("Cards")]
    [Space(10)]
    [Header("Commons")]
    public int commonCardPercentage;
    public int commonCardPrice;
    public List<CardType> commonCards;

    [Header("Uncoommons")]
    public int uncommonCardPercentage;
    public int uncommonCardPrice;
    public List<CardType> uncommonCards;

    [Header("Rares")]
    public int rareCardPercentage;
    public int rareCardPrice;
    public List<CardType> rareCards;

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
    public int rerollShopPrice;
    public int upgradeShopPrice;
    public List<Vector3> cardLocations;
    public List<Vector3> keepsakeLocations;
    public ShopEncounterEvent shopEncounterEvent;

    public int getTotalCardPercentage() {
        return commonCardPercentage + 
            uncommonCardPercentage +   
            rareCardPercentage;
    }

    public int getTotalCompanionPercentage() {
        return commonCompanionPercentage +
            uncommonCompanionPercentage +
            rareCompanionPercentage;
    }
}
