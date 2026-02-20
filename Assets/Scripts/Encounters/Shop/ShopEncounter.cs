using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardInShopWithPrice
{
    public CardType cardType;
    public int price;

    public CardPoolSO cardPool;

    // Nullable for neutral cards.
    public CompanionTypeSO sourceCompanion;

    public Card.CardRarity rarity;

    public Sprite genericArtwork;
    public PackSO packSO;
    public bool increasedPrice;

    public CardInShopWithPrice(
        CardType cardType,
        int price,
        CompanionTypeSO companionType,
        CardPoolSO cardPool,
        Card.CardRarity rarity,
        PackSO packSO
    )
    {
        this.cardType = cardType;
        this.price = price;
        this.sourceCompanion = companionType;
        this.cardPool = cardPool;
        this.rarity = rarity;
        this.packSO = packSO;
        this.increasedPrice = false;
    }
}

[System.Serializable]
public class CompanionInShopWithPrice {
    public CompanionTypeSO companionType;
    public int price;
    public bool increasedPrice;
    public int sustainedDamage = 0;
    public int maxHealthBonus = 0;
    public CompanionRarity rarity;

    public CompanionInShopWithPrice(CompanionTypeSO companionType, int price, int sustainedDamage, int maxHealthBonus, CompanionRarity rarity)
    {
        this.companionType = companionType;
        this.price = price;
        this.increasedPrice = false;
        this.sustainedDamage = sustainedDamage;
        this.maxHealthBonus = maxHealthBonus;
        this.rarity = rarity;
    }
}

[System.Serializable]
public class ShopEncounter : Encounter
{
    public ShopDataSO shopData;
    public List<CardInShopWithPrice> cardsInShop = new List<CardInShopWithPrice>();
    public List<CompanionInShopWithPrice> companionsInShop = new List<CompanionInShopWithPrice>();
    private EncounterConstantsSO encounterConstants;

    private ShopManager shopManager;

    public ShopEncounter() {
        this.encounterType = EncounterType.Shop;
    }

    public ShopEncounter(ShopDataSO shopData) {
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
    }

    public ShopEncounter(ShopEncounterSerializable shopEncounterSerializable, SORegistry registry, ShopDataSO shopData) {
        this.act = shopEncounterSerializable.act;
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
    }

    public void Build(
            ShopManager shopManager,
            List<Companion> companionList,
            EncounterConstantsSO constants,
            ShopLevel shopLevel,
            StaticShopEncounter staticEncounter = null)
    {
        this.shopManager = shopManager;
        this.encounterConstants = constants;
        this.encounterType = EncounterType.Shop;
        generateShopEncounter(shopLevel, companionList, staticEncounter);
        cardsInShop.ForEach(card => shopManager.shopViewController.AddCardToShopView(card));
        companionsInShop.ForEach(companion => shopManager.shopViewController.AddCompanionToShopView(companion));
        shopManager.shopViewController.AddCompanionShineAfterHeirarchyChanged();
        shopManager.SetupUnitManagement();
    }

    public void Reroll(List<Companion> companionList, ShopLevel shopLevel) {
        generateShopEncounter(shopLevel, companionList);
        shopManager.shopViewController.Reroll(cardsInShop, companionsInShop);
    }

    public override void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder) {
        encounterBuilder.BuildShopEncounter(this);
    }

    private void generateShopEncounter(ShopLevel shopLevel, List<Companion> companionList, StaticShopEncounter staticEncounter = null) {
        cardsInShop = new List<CardInShopWithPrice>();
        companionsInShop = new List<CompanionInShopWithPrice>();

        generateCards(shopLevel, companionList, staticEncounter);
        generateKeepsakes(shopLevel, companionList, staticEncounter);
    }

    private void generateCards(ShopLevel shopLevel, List<Companion> companionList, StaticShopEncounter staticEncounter = null) {
        if (staticEncounter != null && staticEncounter.cardTypes != null && staticEncounter.cardTypes.Count > 0) {
            foreach (CardType cardType in staticEncounter.cardTypes) {
                Card.CardRarity rarity = DetermineCardRarity(cardType);
                int price = GetCardPriceForRarity(rarity);
                CardInShopWithPrice card = new CardInShopWithPrice(cardType, price, null, null, rarity, null);
                int bonusCost = AddBonusCardCost();
                card.price += bonusCost;
                card.increasedPrice = bonusCost > 0;
                cardsInShop.Add(card);
            }
            return;
        }

        ShopCardProbabilityDistBuilder b = new(
            shopData, shopLevel, companionList
        );
        ShopProbabilityDistribution dist = b.Build();
        dist.PrintCardDistribution();
        List<CardInShopWithPrice> chosen = dist.ChooseWithoutReplacement(shopLevel.numCardsToShow);
        foreach (CardInShopWithPrice z in chosen) {
            int bonusCost = AddBonusCardCost();
            z.price += bonusCost;
            z.increasedPrice = bonusCost > 0;
            cardsInShop.Add(z);
        }
    }

    private Card.CardRarity DetermineCardRarity(CardType cardType) {
        // Search through all card pools (neutral, pack, and companion)
        // to determine which rarity tier a card belongs to.
        List<CardPoolSO> pools = new() { shopData.neutralCardPool };
        foreach (PackSO pack in shopData.activePacks) {
            if (pack.packCardPoolSO != null) pools.Add(pack.packCardPoolSO);
        }
        // Also check companion-specific card pools.
        List<CompanionTypeSO> allCompanionTypes = new();
        allCompanionTypes.AddRange(shopData.companionPool.commonCompanions);
        allCompanionTypes.AddRange(shopData.companionPool.uncommonCompanions);
        allCompanionTypes.AddRange(shopData.companionPool.rareCompanions);
        foreach (CompanionTypeSO companion in allCompanionTypes) {
            if (companion.cardPool != null && !pools.Contains(companion.cardPool))
                pools.Add(companion.cardPool);
        }

        foreach (CardPoolSO pool in pools) {
            if (pool.commonCards.Contains(cardType) || pool.unlockableCommonCards.Contains(cardType))
                return Card.CardRarity.COMMON;
            if (pool.uncommonCards.Contains(cardType) || pool.unlockableUncommonCards.Contains(cardType))
                return Card.CardRarity.UNCOMMON;
            if (pool.rareCards.Contains(cardType) || pool.unlockableRareCards.Contains(cardType))
                return Card.CardRarity.RARE;
        }

        Debug.LogWarning($"Could not determine rarity for card {cardType.name}, defaulting to COMMON");
        return Card.CardRarity.COMMON;
    }

    private int GetCardPriceForRarity(Card.CardRarity rarity) {
        return rarity switch {
            Card.CardRarity.COMMON => shopData.commonCardPrice,
            Card.CardRarity.UNCOMMON => shopData.uncommonCardPrice,
            Card.CardRarity.RARE => shopData.rareCardPrice,
            _ => shopData.commonCardPrice,
        };
    }

    public void generateKeepsakes(ShopLevel shopLevel, List<Companion> team, StaticShopEncounter staticEncounter = null)
    {
        if (staticEncounter != null && staticEncounter.companionTypes != null && staticEncounter.companionTypes.Count > 0) {
            foreach (CompanionTypeSO companionType in staticEncounter.companionTypes) {
                CompanionRarity rarity = DetermineCompanionRarity(companionType);
                int maxHealthBonus = shopLevel.ratBonusHealth;
                CompanionInShopWithPrice keepsake = new CompanionInShopWithPrice(
                    companionType, shopData.companionKeepsakePrice, 0, maxHealthBonus, rarity
                );
                int bonusCost = AddBonusRatCost();
                keepsake.price += bonusCost;
                if (bonusCost > 0) keepsake.increasedPrice = true;
                companionsInShop.Add(keepsake);
            }
            return;
        }

        int numKeepsakesToGenerate = shopLevel.numKeepsakesToShow;

        // Maintain a list of the keepsakes that are out of the shop pool.
        // All companion types on your team are not considered in the pool.
        List<CompanionTypeSO> keepsakesOutOfPool = team.Select(x => x.companionType).ToList();

        for (int i = 0; i < numKeepsakesToGenerate; i++)
        {
            // Determine what the companion pool is for this single keepsake being generated
            CompanionRarity rarity = PickRarity(
                shopLevel.commonCompanionPercentage,
                shopLevel.uncommonCompanionPercentage,
                shopLevel.rareCompanionPercentage,
                shopData.companionPool.commonCompanions.Count > 0,
                shopData.companionPool.uncommonCompanions.Count > 0,
                shopData.companionPool.rareCompanions.Count > 0
            );

            List<CompanionTypeSO> companions = new List<CompanionTypeSO>();
            switch (rarity)
            {
                case CompanionRarity.COMMON:
                    companions = shopData.companionPool.commonCompanions;
                    break;

                case CompanionRarity.UNCOMMON:
                    companions = shopData.companionPool.uncommonCompanions;
                    break;

                case CompanionRarity.RARE:
                    companions = shopData.companionPool.rareCompanions;
                    break;
            }

            // Simple weighted sampling algorithm: N copies of each companion type in the list,
            // where N corresponds to their weight.
            // Then we pick one uniformly at random.
            List<CompanionTypeSO> companionSampleDist = new();
            foreach (CompanionTypeSO c in companions)
            {
                // "Scarcity" mechanic; we reduce the number of companions
                // available by removing the keepsake count after pool.
                int keepsakeCopies = ProgressManager.Instance.IsFeatureEnabled(AscensionType.SCARCE_SHOPS)
                    ? shopData.numKeepsakeCopies - (int)ProgressManager.Instance.GetAscensionSO(AscensionType.SCARCE_SHOPS).ascensionModificationValues.GetValueOrDefault("numReduced", 3f)
                    : shopData.numKeepsakeCopies;
                int numAvailable = keepsakeCopies - numCompanionsOfType(keepsakesOutOfPool, c);
                // in the case, where we exhaust all the companions of a given type, let there
                // be 1 available always, just so it is possible but much less likely.
                numAvailable = Math.Max(1, numAvailable);
                companionSampleDist.AddRange(Enumerable.Repeat(c, numAvailable));
                Debug.Log("Shop scarcity. " + c.name + ": " + numAvailable.ToString());
            }

            // Pick a keepsake from the sample distribution and add it to the shop's cards
            int number = UnityEngine.Random.Range(0, companionSampleDist.Count);
            CompanionTypeSO selected = companionSampleDist[number];
            int sustainedDamage = 0;
            if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.DAMAGED_COMPANIONS))
            {
                // Only a proportion of the companions shown in the shop will be damaged.
                if (UnityEngine.Random.Range(0f, 1f) <= ProgressManager.Instance.GetAscensionSO(AscensionType.DAMAGED_COMPANIONS).
                    ascensionModificationValues.GetValueOrDefault("pctDamaged", 0f))
                {
                    float min = ProgressManager.Instance.GetAscensionSO(AscensionType.DAMAGED_COMPANIONS).
                    ascensionModificationValues.GetValueOrDefault("healthReductionMin", 0f);
                    float max = ProgressManager.Instance.GetAscensionSO(AscensionType.DAMAGED_COMPANIONS).
                    ascensionModificationValues.GetValueOrDefault("healthReductionMax", 0f);
                    // Distribute the damage to the companion uniformly at random.
                    sustainedDamage = (int)UnityEngine.Random.Range(min, max);
                }
            }
            int maxHealthBonus = shopLevel.ratBonusHealth;

            CompanionInShopWithPrice keepsake = new CompanionInShopWithPrice(
                selected, shopData.companionKeepsakePrice, sustainedDamage, maxHealthBonus, rarity
            );
            int bonusCost = AddBonusRatCost();
            keepsake.price += bonusCost;
            if (bonusCost > 0) keepsake.increasedPrice = true;
            companionsInShop.Add(keepsake);
            keepsakesOutOfPool.Add(selected);
        }
    }

    private int AddBonusRatCost()
    {
        if (!ProgressManager.Instance.IsFeatureEnabled(AscensionType.STINGY_CONCIERGE)) return 0;

        var ratio = ProgressManager.Instance.GetAscensionSO(AscensionType.STINGY_CONCIERGE).
            ascensionModificationValues.GetValueOrDefault("expensiveRatRatio", 0.25f);
        if (UnityEngine.Random.Range(0f, 1f) >= ratio) return 0;

        return (int) ProgressManager.Instance.GetAscensionSO(AscensionType.STINGY_CONCIERGE).
            ascensionModificationValues.GetValueOrDefault("costIncrease", 1f);
    }
    private int AddBonusCardCost()
    {
        if (!ProgressManager.Instance.IsFeatureEnabled(AscensionType.STINGY_CONCIERGE)) return 0;

        var ratio = ProgressManager.Instance.GetAscensionSO(AscensionType.STINGY_CONCIERGE).
            ascensionModificationValues.GetValueOrDefault("expensiveCardRatio", 0.25f);
        if (UnityEngine.Random.Range(0f, 1f) >= ratio) return 0;

        return (int) ProgressManager.Instance.GetAscensionSO(AscensionType.STINGY_CONCIERGE).
            ascensionModificationValues.GetValueOrDefault("costIncrease", 1f);
    }

    private CompanionRarity DetermineCompanionRarity(CompanionTypeSO companionType) {
        if (shopData.companionPool.commonCompanions.Contains(companionType))
            return CompanionRarity.COMMON;
        if (shopData.companionPool.uncommonCompanions.Contains(companionType))
            return CompanionRarity.UNCOMMON;
        if (shopData.companionPool.rareCompanions.Contains(companionType))
            return CompanionRarity.RARE;

        Debug.LogWarning($"Could not determine rarity for companion {companionType.name}, defaulting to COMMON");
        return CompanionRarity.COMMON;
    }

    private int numCompanionsOfType(List<CompanionTypeSO> companions, CompanionTypeSO companionType)
    {
        int count = 0;
        foreach (CompanionTypeSO c in companions)
        {
            if (c == companionType)
            {
                count++;
            }
            // If we have a combined version on the team, that effectively
            // means we bought 3 of the same kind.
            if (c == companionType.upgradeTo)
            {
                count += 3;
            }
            if (c == companionType.upgradeTo.upgradeTo)
            {
                count += 6;
            }
        }
        return count;
    }

    private CompanionRarity PickRarity(
            int commonPercent,
            int uncommonPercent,
            int rarePercent,
            bool commons,
            bool uncommons,
            bool rares) {
        List<CompanionRarity> rarityPool = new List<CompanionRarity>();
        List<int> percents = new List<int>();
        if (commons) {
            rarityPool.Add(CompanionRarity.COMMON);
            percents.Add(commonPercent);
        }

        if (uncommons) {
            rarityPool.Add(CompanionRarity.UNCOMMON);
            percents.Add(uncommonPercent);
        }

        if (rares) {
            rarityPool.Add(CompanionRarity.RARE);
            percents.Add(rarePercent);
        }

        int totalPercentage = 0;
        foreach (int i in percents) {
            totalPercentage = totalPercentage + i;
        }

        int randomNumber = UnityEngine.Random.Range(0, totalPercentage); // min inclusive, max exclusive
        int currentPercentageCheck = 0;
        for (int i = 0; i < percents.Count; i++) {
            currentPercentageCheck = currentPercentageCheck + percents[i];
            if (randomNumber < currentPercentageCheck) {
                return rarityPool[i];
            }
        }
        Debug.LogError("ShopEncounter:PickRarity: Failed to pick rarity. " +
            "It's likely that the card pool for this companion is empty");

        // Should never hit this case
        return CompanionRarity.COMMON;
    }

    private enum Rarity {
        Common,
        Uncommon,
        Rare
    }

}

[System.Serializable]
public class ShopEncounterSerializable : EncounterSerializable {
    public ShopEncounterSerializable(ShopEncounter encounter) : base(encounter) {
    }
}