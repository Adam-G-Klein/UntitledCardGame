using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop",
    menuName = "Encounters/Shop/New Shop")]
public class ShopDataSO : ScriptableObject
{
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
    [Header("Misc")]
    public List<Vector3> cardLocations;

    public int getTotalCardPercentage() {
        return commonCardPercentage + 
            uncommonCardPercentage +   
            rareCardPercentage;
    }
}
