using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnlockableCard
{
    [SerializeReference]
    public CardType cardType;

    public bool isUnlocked = false;
}

[CreateAssetMenu(
    fileName = "NewUnlockableCardPool",
    menuName = "Cards/Unlockable Card Pool")]
public class UnlockableCardPoolSO: ScriptableObject
{
    [SerializeField]
    private List<UnlockableCard> commonCards;
    [SerializeField]
    private List<UnlockableCard> uncommonCards;
    [SerializeField]
    private List<UnlockableCard> rareCards;

    // Get accessors for common cards, uncommon cards, and rare cards, only returning unlocked cards.
    public List<CardType> CommonCards
    {
        get
        {
            return commonCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
        }
    }

    public List<CardType> UncommonCards
    {
        get
        {
            return uncommonCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
        }
    }

    public List<CardType> RareCards
    {
        get
        {
            return rareCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
        }
    }
}
