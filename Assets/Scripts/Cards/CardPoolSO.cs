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

    public List<CardType> AllUnlockedCommonCards(List<CardType> unlockedCards)
    {
        List<CardType> cards = new List<CardType>(commonCards);
        foreach (CardType card in unlockableCommonCards)
        {
            if (unlockedCards.Contains(card))
            {
                cards.Add(card);
            }
        }
        return cards;
    }

    public List<CardType> AllUnlockedUncommonCards(List<CardType> unlockedCards)
    {
        List<CardType> cards = new List<CardType>(uncommonCards);
        foreach (CardType card in unlockableUncommonCards)
        {
            if (unlockedCards.Contains(card))
            {
                cards.Add(card);
            }
        }
        return cards;
    }

    public List<CardType> AllUnlockedRareCards(List<CardType> unlockedCards)
    {
        List<CardType> cards = new List<CardType>(rareCards);
        foreach (CardType card in unlockableRareCards)
        {
            if (unlockedCards.Contains(card))
            {
                cards.Add(card);
            }
        }
        return cards;
    }
}
