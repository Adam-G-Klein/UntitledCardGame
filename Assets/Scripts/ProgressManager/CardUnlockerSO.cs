using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CardUnlockerSO : ScriptableObject {
    public abstract List<CardType> ChooseCardsToUnlock(GameStateVariableSO gameState, List<CardType> unlockedCards, int numberToUnlock);

    // Every unlockable card in the given packs: the pack's own card pool plus the card pools of every companion in the pack.
    public static List<CardType> GetUnlockableCards(IEnumerable<PackSO> packs) {
        List<CardType> cards = new List<CardType>();
        if (packs == null) return cards;
        foreach (PackSO pack in packs) {
            if (pack == null) continue;
            AddUnlockableCards(cards, pack.packCardPoolSO);
            foreach (CompanionTypeSO companionType in AllCompanionsInPack(pack)) {
                AddUnlockableCards(cards, companionType.cardPool);
            }
        }
        return cards.Distinct().ToList();
    }

    protected static IEnumerable<CompanionTypeSO> AllCompanionsInPack(PackSO pack) {
        CompanionPoolSO pool = pack.companionPoolSO;
        if (pool == null) return Enumerable.Empty<CompanionTypeSO>();
        return pool.commonCompanions.Concat(pool.uncommonCompanions).Concat(pool.rareCompanions).Where(c => c != null);
    }

    protected static IEnumerable<CardType> UnlockableCardsInPool(CardPoolSO pool) {
        if (pool == null) return Enumerable.Empty<CardType>();
        return pool.unlockableRareCards.Concat(pool.unlockableUncommonCards).Concat(pool.unlockableCommonCards);
    }

    private static void AddUnlockableCards(List<CardType> cards, CardPoolSO pool) {
        cards.AddRange(UnlockableCardsInPool(pool));
    }
}
