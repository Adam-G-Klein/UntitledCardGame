using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardWithWeight {
    public CardInShopWithPrice card;
    public double weight;
    public Card.CardRarity rarity;
    public CardSource cardSource;

    public CardWithWeight(
        CardInShopWithPrice card,
        double weight,
        Card.CardRarity rarity,
        CardSource cardSource
    ) {
        this.card = card;
        this.weight = weight;
        this.rarity = rarity;
        this.cardSource = cardSource;
    }

    // Reweighting the card.
    public CardWithWeight(
        CardWithWeight x,
        double weight
    ) {
        this.card = x.card;
        this.weight = weight;
        this.rarity = x.rarity;
        this.cardSource = x.cardSource;
    }
}

public enum CardSource {
    CompanionPool,
    NeutralPool,
    PackPool,
    CrossPackPool,
}

public class ShopCardProbabilityDistBuilder {

    List<CardWithWeight> filteredCards = new();

    Dictionary<Card.CardRarity, double> rarityProb = new();
    Dictionary<CardSource, double> cardSourceProb = new();

    public ShopCardProbabilityDistBuilder(ShopDataSO shopData, ShopLevel shopLevel, List<Companion> companionList) {
        // Add the card pools for each companion that is on your team.
        List<ValueTuple<CardPoolSO, CompanionTypeSO, PackSO>> companionCardPools = new();
        List<ValueTuple<CardPoolSO, CompanionTypeSO, PackSO>> packCardPools = new();
        List<CardPoolSO> crossPackCardPools = new();
        foreach (Companion companion in companionList)
        {
            companionCardPools.Add((companion.companionType.cardPool, companion.companionType, null));
            packCardPools.Add((companion.companionType.pack.packCardPoolSO, null, companion.companionType.pack));
        }
        foreach (CrossPackCardPoolSO crossPackPool in shopData.activeCrossPackCardPools)
        {
            // Check if BOTH packs are present in the companion list.
            // Inefficient search but there are only going to be maximum 10 companions.
            bool pack1Present = companionList.Any(c => c.companionType.pack == crossPackPool.associatedPack1);
            bool pack2Present = companionList.Any(c => c.companionType.pack == crossPackPool.associatedPack2);
            if (pack1Present && pack2Present)
            {
                crossPackCardPools.Add(crossPackPool.crossPackCardPool);
            }
        }

        List<CardWithWeight> allCards = new();
        foreach (ValueTuple<CardPoolSO, CompanionTypeSO, PackSO> tup in companionCardPools) {
            var x = ShopCardProbabilityDistBuilder.ExtractCardsFromPool(
                shopData,
                tup.Item1,
                tup.Item2,
                tup.Item3,
                CardSource.CompanionPool
            );
            allCards.AddRange(x);
        }
        foreach (ValueTuple<CardPoolSO, CompanionTypeSO, PackSO> tup in packCardPools) {
            var x = ShopCardProbabilityDistBuilder.ExtractCardsFromPool(
                shopData,
                tup.Item1,
                tup.Item2,
                tup.Item3,
                CardSource.PackPool
            );
            allCards.AddRange(x);
        }
        foreach (CardPoolSO pool in crossPackCardPools) {
            var x = ShopCardProbabilityDistBuilder.ExtractCardsFromPool(
                shopData,
                pool,
                null,
                null,
                CardSource.CrossPackPool
            );
            allCards.AddRange(x);
        }

        var y = ShopCardProbabilityDistBuilder.ExtractCardsFromPool(
                shopData,
                shopData.neutralCardPool,
                null,
                null,
                CardSource.NeutralPool
            );
        allCards.AddRange(y);

        // Filter out cards with the same card type that are added from multiple card pools.
        // There will likely be multiple of the same card pool in the list,
        // because you can have duplicate companions and even different companions will share
        // the same pack pool.
        this.filteredCards = allCards
            .GroupBy(x => x.card.cardType)
            .Select(g => g.First())
            .ToList();

        rarityProb[Card.CardRarity.COMMON] = (double) shopLevel.commonCardPercentage / 100.0;
        rarityProb[Card.CardRarity.UNCOMMON] = (double) shopLevel.uncommonCardPercentage / 100.0;
        rarityProb[Card.CardRarity.RARE] = (double) shopLevel.rareCardPercentage / 100.0;

        cardSourceProb[CardSource.CompanionPool] = (double) shopData.companionTypeCardPoolPct / 100.0;
        cardSourceProb[CardSource.PackPool] = (double) shopData.packCardPoolPct / 100.0;
        cardSourceProb[CardSource.CrossPackPool] = (double) shopData.crossPackCardPoolPct / 100.0;
        cardSourceProb[CardSource.NeutralPool] = (double) shopData.neutralCardPoolPct / 100.0;
    }

    public static List<CardWithWeight> ExtractCardsFromPool(
        ShopDataSO shopData,
        CardPoolSO pool,
        CompanionTypeSO dude,
        PackSO packSO,
        CardSource cardSource
    ) {
        List<CardWithWeight> cards = new();
        List<CardType> commonPool = pool.AllUnlockedCommonCards(ProgressManager.Instance.progressSO.unlockedCards, shopData.alwaysShowUnlockedCards);
        List<CardType> uncommonPool = pool.AllUnlockedUncommonCards(ProgressManager.Instance.progressSO.unlockedCards, shopData.alwaysShowUnlockedCards);
        List<CardType> rarePool = pool.AllUnlockedRareCards(ProgressManager.Instance.progressSO.unlockedCards, shopData.alwaysShowUnlockedCards);
        foreach (CardType card in commonPool) {
            CardInShopWithPrice c = new CardInShopWithPrice(card, shopData.commonCardPrice, dude, pool, Card.CardRarity.COMMON, packSO);
            cards.Add(new CardWithWeight(c, 1.0, c.rarity, cardSource));
        }
        foreach (CardType card in uncommonPool) {
            CardInShopWithPrice c = new CardInShopWithPrice(card, shopData.uncommonCardPrice, dude, pool, Card.CardRarity.UNCOMMON, packSO);
            cards.Add(new CardWithWeight(c, 1.0, c.rarity, cardSource));
        }
        foreach (CardType card in rarePool) {
            CardInShopWithPrice c = new CardInShopWithPrice(card, shopData.rareCardPrice, dude, pool, Card.CardRarity.RARE, packSO);
            cards.Add(new CardWithWeight(c, 1.0, c.rarity, cardSource));
        }
        return cards;
    }

    private int CountCardsWithSameFeatures(CardWithWeight target) {
        return filteredCards.Count(card =>
            card.rarity == target.rarity &&
            card.cardSource == target.cardSource &&
            (card.cardSource != CardSource.CompanionPool || card.card.sourceCompanion == target.card.sourceCompanion) &&
            (card.cardSource != CardSource.PackPool || card.card.packSO == target.card.packSO)
        );
    }

    private int CountNumGroupsForRarityAndCardSource(Card.CardRarity rarity, CardSource cardSource) {
        return filteredCards
            .Where(card =>
                card.rarity == rarity &&
                card.cardSource == cardSource
            )
            .Select(card =>
                // Group by companion type if from companion pool.
                card.cardSource == CardSource.CompanionPool ?
                    card.card.sourceCompanion.name :
                // Group by pack if from pack pool.
                card.cardSource == CardSource.PackPool ?
                    card.card.packSO.name :
                // Otherwise, group all neutral cards together.
                    "neutral"
            )
            .Distinct()
            .Count();
    }

    // This is where we assign the probabilities to each card.
    // We have overall probabilities for each category:
    // - rarity probability (each shop level has different probs for common, uncommon, rare)
    // - card source probability (allows us to control how many neutral cards vs. pack vs. companion type the player sees)
    // Then, we have the count of each card with the same exact features.
    // For now, we divide that space equally.
    // We also have "groups" for each card source and rarity; for example, all the packs with a common card.
    // We will weight the packs evenly, no matter how many cards they have :)
    // The sum of the probabilities in the returned list should sum to one.
    public ShopProbabilityDistribution Build() {
        List<CardWithWeight> cardCategoricalDistribution = new();
        foreach (CardWithWeight card in filteredCards) {
            // Count the numer of groups for this rarity and card source.
            int numGroups = this.CountNumGroupsForRarityAndCardSource(card.rarity, card.cardSource);
            int count = this.CountCardsWithSameFeatures(card);
            // Debug.Log("Number of cards with same features: " + count + ", card " + card.card.cardType.name + ", number of groups: " + numGroups);
            double probInGroup = 1.0 / (double) count;
            double groupProb = 1.0 / (double) numGroups;  // Each group gets equal probability mass.
            double combinedProb = probInGroup * groupProb * rarityProb[card.rarity] * cardSourceProb[card.cardSource];
            cardCategoricalDistribution.Add(new CardWithWeight(card, combinedProb));
        }
        return new ShopProbabilityDistribution(cardCategoricalDistribution);
    }
}

public class ShopProbabilityDistribution {
    List<CardWithWeight> cardCategoricalDistribution = new();

    private static System.Random rng = new System.Random();

    public ShopProbabilityDistribution(List<CardWithWeight> cardCat) {
        this.cardCategoricalDistribution = cardCat;
    }

    public List<CardInShopWithPrice> ChooseWithReplacement(int count) {
        List<CardWithWeight> dist = new List<CardWithWeight>(cardCategoricalDistribution);

        List<CardInShopWithPrice> x = new();
        for (int i = 0; i < count; i++) {
            x.Add(randomDraw(dist).card);
        }
        return x;
    }

    public List<CardInShopWithPrice> ChooseWithoutReplacement(int count) {
        List<CardWithWeight> dist = new List<CardWithWeight>(cardCategoricalDistribution);

        List<CardInShopWithPrice> x = new();
        // Simple draw then remove from the copied list.
        for (int i = 0; i < count; i++) {
            CardWithWeight drawn = randomDraw(dist);
            x.Add(drawn.card);
            dist.Remove(drawn);
        }
        return x;
    }


    private CardWithWeight randomDraw(List<CardWithWeight> cards) {
        double totalWeight = cards.Sum(c => (double)c.weight);
        // NextDouble() returns a native double in [0, 1), avoiding the
        // float precision loss that UnityEngine.Random.Range() would introduce.
        double sample = rng.NextDouble() * totalWeight; // or use System.Random for a double draw
        double accum = 0.0;
        for (int i = 0; i < cards.Count; i++) {
            accum += cards[i].weight;
            if (sample <= accum) {
                return cards[i];
            }
        }
        return cards[cards.Count - 1]; // fallback instead of null
    }

    public void PrintCardDistribution()
    {
        List<CardWithWeight> sortedDist = this.cardCategoricalDistribution
            .OrderByDescending(c => c.weight)
            .ToList();

        foreach (CardWithWeight card in sortedDist)
        {
            Debug.Log($"[Card Probability Distribution] {card.card.cardType.name} ({card.rarity}, {card.cardSource}): {card.weight}");
        }
        // Check that the probabilities sum to 1.
        double totalProb = sortedDist.Select(c => c.weight).ToList().Sum();
        Debug.Log($"[Card Probability Distribution] Total Probability: {totalProb}");
    }
}
