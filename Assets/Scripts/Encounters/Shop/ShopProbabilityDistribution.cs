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

public class ShopCardProbabilityDistBuilder {
    Dictionary<Card.CardRarity, List<CardWithWeight>> rarityGrouping = new();
    public void AddCard(CardInShopWithPrice card) {
        if (!rarityGrouping.ContainsKey(card.rarity)) {
            rarityGrouping.Add(card.rarity, new List<CardWithWeight>());
        }
        int existing = rarityGrouping[card.rarity].FindIndex(c => c.card.cardType == card.cardType);
        if (existing < 0) {
            rarityGrouping[card.rarity].Add(new CardWithWeight(card, 1f));
        } else {
            // rarityGrouping[card.rarity][existing].weight += 1f;
        }
    }

    public ShopProbabilityDistribution Build(ShopLevel shopLevel) {
        List<CardWithWeight> cardCategoricalDistribution = new();
        cardCategoricalDistribution.AddRange(getRarityConditionalDist(Card.CardRarity.COMMON, (float) shopLevel.commonCardPercentage / 100f));
        cardCategoricalDistribution.AddRange(getRarityConditionalDist(Card.CardRarity.UNCOMMON, (float) shopLevel.uncommonCardPercentage / 100f));
        cardCategoricalDistribution.AddRange(getRarityConditionalDist(Card.CardRarity.RARE, (float) shopLevel.rareCardPercentage / 100f));
        return new ShopProbabilityDistribution(cardCategoricalDistribution);
    }

    private List<CardWithWeight> getRarityConditionalDist(Card.CardRarity rarity, float rarityPct) {
        List<CardWithWeight> dist = new();
        foreach (CardWithWeight c in rarityGrouping[rarity]) {
            // Divide each card's weight by the sum of all the weights in the group.
            // This has the effect of weighting cards from packs more heavily.
            // This will sum up to 1.
            float occurenceWeight = c.weight / rarityGrouping[Card.CardRarity.COMMON].Select(c => c.weight).Sum();

            // The total probability of the card will be the product of the rarity % and the occurence % conditioned on the rarity.
            dist.Add(new CardWithWeight(c.card, rarityPct * occurenceWeight));
        }
        return dist;
    }
}

public class ShopProbabilityDistribution {
    List<CardWithWeight> cardCategoricalDistribution = new();

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

    public void PrintCardDistribution()
    {
        Debug.Log("🃏 Card Distribution:");
        foreach (CardWithWeight card in cardCategoricalDistribution)
        {
            Debug.Log($"- {card.card.cardType.name}: {card.weight}");
        }
    }
}
