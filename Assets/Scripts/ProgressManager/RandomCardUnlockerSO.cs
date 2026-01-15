using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomCardUnlocker", 
    menuName = "Cards/Card Unlocker/Random Card Unlocker")]
public class RandomCardUnlockerSO : CardUnlockerSO {
    public int numberToUnlock;

    public override List<CardType> ChooseCardsToUnlock(GameStateVariableSO gameState) {
        List<CardType> cardsToUnlock = new List<CardType>();

        List<PackSO> activePacks = gameState.baseShopData.activePacks;
        List<CompanionTypeSO> companionTypes = gameState.companions.activeCompanions
            .Select(c => c.companionType)
            .ToList();
        
        List<CardType> unlockableCards = new List<CardType>();
        foreach (var pack in activePacks)
        {
            unlockableCards.AddRange(pack.packCardPoolSO.unlockableRareCards);
            unlockableCards.AddRange(pack.packCardPoolSO.unlockableUncommonCards);
            unlockableCards.AddRange(pack.packCardPoolSO.unlockableCommonCards);
        }
        foreach (var companionType in companionTypes) {
            unlockableCards.AddRange(companionType.cardPool.unlockableRareCards);
            unlockableCards.AddRange(companionType.cardPool.unlockableUncommonCards);
            unlockableCards.AddRange(companionType.cardPool.unlockableCommonCards);
        }

        // Convert the list to a set for O(1) lookup when checking which cards are unlocked
        HashSet<CardType> alreadyUnlockedCards = new HashSet<CardType>();
        gameState.unlockedCards.ForEach(c => alreadyUnlockedCards.Add(c));

        // Filter out cards that are already unlocked.
        unlockableCards = unlockableCards.FindAll(x => !alreadyUnlockedCards.Contains(x));
        if (unlockableCards.Count == 0)
        {
            Debug.Log("All cards are already unlocked.");
            return cardsToUnlock;
        }

        // Shuffle the list
        for (int i = 0; i < unlockableCards.Count; i++)
        {
            CardType temp = unlockableCards[i];
            int randomIndex = UnityEngine.Random.Range(i, unlockableCards.Count);
            unlockableCards[i] = unlockableCards[randomIndex];
            unlockableCards[randomIndex] = temp;
        }

        for (int i = 0; i < numberToUnlock && i < unlockableCards.Count; i++)
        {
            Debug.Log($"Choosing card to unlock: {unlockableCards[i].Name}");
            cardsToUnlock.Add(unlockableCards[i]);
        }

        return cardsToUnlock;
    }
}