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

    public Sprite genericCardIconSprite;

    public List<CardType> unlockableCommonCards;
    public List<CardType> unlockableUncommonCards;
    public List<CardType> unlockableRareCards;
}
