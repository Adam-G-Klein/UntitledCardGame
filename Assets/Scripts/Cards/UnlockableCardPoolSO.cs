using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnlockableCard
{
    // TODO: Make this much more serialization-safe, backwards compatible, etc.
    // Unclear how well this will work with future changes to the CardType.
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

    public List<UnlockableCard> GetAllUnlockableCards()
    {
        return commonCards.Concat(uncommonCards).Concat(rareCards).ToList();
    }

    public List<CardType> CommonCards => commonCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
    public List<CardType> UncommonCards => uncommonCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
    public List<CardType> RareCards => rareCards.Where(x => x.isUnlocked).Select(x => x.cardType).ToList();
}
