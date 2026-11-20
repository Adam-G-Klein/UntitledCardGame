using System.Collections.Generic;
using UnityEngine;

// Used by GenerateRun for the Card Test build: pulls every card out of
// the listed card pools (plus any explicitly added cards) so that the
// test rats' starting decks contain every card we want to be able to test.
[CreateAssetMenu(
    fileName = "CardTestConfiguration",
    menuName = "Decks/Card Test Configuration")]
public class CardTestConfigurationSO : ScriptableObject
{
    // The companions on the test team. If empty, GenerateRun's testStaticCompanions are used instead.
    public List<CompanionTypeSO> companionTypes;

    public List<CardPoolSO> cardPools;

    // Cards to add to the starting deck that aren't pulled from a card pool.
    public List<CardType> additionalStartingDeckCards;

    public bool includeUnlockableCards = true;

    public List<CardType> GetAllCards()
    {
        List<CardType> cards = new List<CardType>(additionalStartingDeckCards);
        foreach (CardPoolSO cardPool in cardPools)
        {
            cards.AddRange(cardPool.AllUnlockedCommonCards(null, includeAll: includeUnlockableCards));
            cards.AddRange(cardPool.AllUnlockedUncommonCards(null, includeAll: includeUnlockableCards));
            cards.AddRange(cardPool.AllUnlockedRareCards(null, includeAll: includeUnlockableCards));
        }
        return cards;
    }
}
