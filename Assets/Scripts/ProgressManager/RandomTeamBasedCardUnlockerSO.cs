using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomTeamBasedCardUnlocker",
    menuName = "Cards/Card Unlocker/Random Team Based Card Unlocker")]
public class RandomTeamBasedCardUnlockerSO : CardUnlockerSO {
    public override List<CardType> ChooseCardsToUnlock(GameStateVariableSO gameState, List<CardType> unlockedCards, int numberToUnlock) {
        List<CardType> cardsToUnlock = new List<CardType>();

        // Normalize to base companion types so upgraded companions count as the same companion.
        HashSet<CompanionTypeSO> teamCompanions = new HashSet<CompanionTypeSO>(
            gameState.companions.activeCompanions.Select(c => BaseType(c.companionType)));
        HashSet<PackSO> teamPacks = new HashSet<PackSO>(
            teamCompanions.Select(c => c.pack).Where(p => p != null));

        // Preferred: cards from companions on the team, plus the packs those companions belong to.
        // Records where each card came from, for logging. The first source found wins.
        Dictionary<CardType, string> cardSources = new Dictionary<CardType, string>();

        List<CardType> preferredCards = new List<CardType>();
        foreach (CompanionTypeSO companionType in teamCompanions) {
            AddUnlockableCards(preferredCards, companionType.cardPool, cardSources, $"team companion {companionType.companionName}");
        }
        foreach (PackSO pack in teamPacks) {
            AddUnlockableCards(preferredCards, pack.packCardPoolSO, cardSources, $"team pack {pack.packName}");
        }

        // Fallback: active packs not represented on the team, and companions not on the team.
        List<CardType> fallbackCards = new List<CardType>();
        foreach (PackSO pack in gameState.activePacks) {
            if (!teamPacks.Contains(pack)) {
                AddUnlockableCards(fallbackCards, pack.packCardPoolSO, cardSources, $"non-team pack {pack.packName}");
            }
            foreach (CompanionTypeSO companionType in AllCompanionsInPack(pack)) {
                if (!teamCompanions.Contains(BaseType(companionType))) {
                    AddUnlockableCards(fallbackCards, companionType.cardPool, cardSources, $"non-team companion {companionType.companionName}");
                }
            }
        }

        // Tracks cards that are unlocked or already chosen, so nothing is picked twice.
        HashSet<CardType> excludedCards = new HashSet<CardType>(unlockedCards);

        ChooseRandomly(preferredCards, excludedCards, cardsToUnlock, numberToUnlock, cardSources, "preferred");
        ChooseRandomly(fallbackCards, excludedCards, cardsToUnlock, numberToUnlock, cardSources, "fallback");

        if (cardsToUnlock.Count == 0)
        {
            Debug.Log("All cards are already unlocked.");
        }
        return cardsToUnlock;
    }

    // Falls back to the type itself if an upgraded companion has no base type assigned.
    private static CompanionTypeSO BaseType(CompanionTypeSO companionType) {
        CompanionTypeSO baseType = companionType.GetBaseCompanionType();
        return baseType != null ? baseType : companionType;
    }

    private static IEnumerable<CompanionTypeSO> AllCompanionsInPack(PackSO pack) {
        CompanionPoolSO pool = pack.companionPoolSO;
        if (pool == null) return Enumerable.Empty<CompanionTypeSO>();
        return pool.commonCompanions.Concat(pool.uncommonCompanions).Concat(pool.rareCompanions);
    }

    private static void AddUnlockableCards(List<CardType> cards, CardPoolSO pool, Dictionary<CardType, string> cardSources, string source) {
        if (pool == null) return;
        foreach (CardType card in pool.unlockableRareCards.Concat(pool.unlockableUncommonCards).Concat(pool.unlockableCommonCards)) {
            cards.Add(card);
            if (!cardSources.ContainsKey(card)) {
                cardSources[card] = source;
            }
        }
    }

    // Shuffles the candidates and adds them to cardsToUnlock until it holds numberToUnlock cards.
    private void ChooseRandomly(List<CardType> candidates, HashSet<CardType> excludedCards, List<CardType> cardsToUnlock, int numberToUnlock, Dictionary<CardType, string> cardSources, string tier) {
        List<CardType> available = candidates.Distinct().Where(c => !excludedCards.Contains(c)).ToList();

        for (int i = 0; i < available.Count; i++)
        {
            CardType temp = available[i];
            int randomIndex = UnityEngine.Random.Range(i, available.Count);
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        for (int i = 0; i < available.Count && cardsToUnlock.Count < numberToUnlock; i++)
        {
            Debug.Log($"Choosing card to unlock: {available[i].GetName()} ({tier}, from {cardSources[available[i]]})");
            cardsToUnlock.Add(available[i]);
            excludedCards.Add(available[i]);
        }
    }
}
