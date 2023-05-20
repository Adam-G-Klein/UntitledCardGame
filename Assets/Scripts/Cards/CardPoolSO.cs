using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardPool",
    menuName = "Cards/Card Pool")]
public class CardPoolSO: ScriptableObject {
    
    public SerializableHashSet<CardType> commonCards;
    public SerializableHashSet<CardType> uncommonCards;
    public SerializableHashSet<CardType> rareCards;

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