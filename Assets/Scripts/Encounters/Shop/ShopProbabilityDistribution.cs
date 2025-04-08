using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardWithWeight {
    public CardInShopWithPrice card;
    public float weight;

    public CardWithWeight(CardInShopWithPrice card, float weight) {
        this.card = card;
        this.weight = weight;
    }
}

public enum CardSource {
    CompanionPool,
    NeutralPool,
    PackPool,
}

public class ShopProbabilityDistribution {
    List<CardWithWeight> cardCategoricalDistribution = new();

    Dictionary<Card.CardRarity, List<CardInShopWithPrice>> rarityGrouping = new();


    public List<CardInShopWithPrice> ChooseWithReplacement(int count) {
        List<CardWithWeight> dist = new List<CardWithWeight>(cardCategoricalDistribution);

        List<CardInShopWithPrice> x = new();
        for (int i = 0; i < count; i++) {
            x.Add(randomDraw(dist).card);
        }
        return x;
    }

    private CardWithWeight randomDraw(List<CardWithWeight> cards) {
        float totalWeight = (float) cards.Select(c => c.weight).ToList().Sum();

        float sample = UnityEngine.Random.Range(0f, totalWeight);
        float accum = 0f;
        for (int i = 0; i < cards.Count; i++) {
            accum += cards[i].weight;
            if (sample <= accum) {
                return cards[i];
            }
        }
        return null;
    }

    public void AddCard(CardInShopWithPrice card) {
        if (!rarityGrouping.ContainsKey(card.rarity)) {
            rarityGrouping.Add(card.rarity, new List<CardInShopWithPrice>());
        }
        if (!rarityGrouping[card.rarity].Any(c => c.cardType == card.cardType)) {
            Debug.LogError("Serious error: duplicate card type added to the shop probability distribution");
        }
        rarityGrouping[card.rarity].Add(card);
    }

    public void Build(ShopLevel shopLevel) {
        foreach (CardInShopWithPrice c in rarityGrouping[Card.CardRarity.COMMON]) {
            float cardPercentage = (float) shopLevel.commonCardPercentage / 100f;
            cardCategoricalDistribution.Add(new CardWithWeight(c, cardPercentage / rarityGrouping[Card.CardRarity.COMMON].Count));
        }
        foreach (CardInShopWithPrice c in rarityGrouping[Card.CardRarity.UNCOMMON]) {
            float cardPercentage = (float) shopLevel.uncommonCardPercentage / 100f;
            cardCategoricalDistribution.Add(new CardWithWeight(c, cardPercentage / rarityGrouping[Card.CardRarity.UNCOMMON].Count));
        }
        foreach (CardInShopWithPrice c in rarityGrouping[Card.CardRarity.RARE]) {
            float cardPercentage = (float) shopLevel.rareCardPercentage / 100f;
            cardCategoricalDistribution.Add(new CardWithWeight(c, cardPercentage / rarityGrouping[Card.CardRarity.RARE].Count));
        }
    }

}
