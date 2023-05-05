using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardPool",
    menuName = "Cards/Card Pool")]
public class CardPoolSO: ScriptableObject {
    
    public List<CardType> commonCards;
    public List<CardType> uncommonCards;
    public List<CardType> rareCards;

    public int commonCardPercentage;
    public int commonCardPrice;
    
    public int uncommonCardPercentage;
    public int uncommonCardPrice;

    public int rareCardPercentage;
    public int rareCardPrice;

    public int getTotalCardPercentage() {
        return commonCardPercentage +
            uncommonCardPercentage +
            rareCardPercentage;
    }

}