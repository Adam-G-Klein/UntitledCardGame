using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardInShopWithPrice {
    public CardType cardType;
    public int price;

    public CardPoolSO cardPool;

    // Nullable for neutral cards.
    public CompanionTypeSO sourceCompanion;

    public Card.CardRarity rarity;

    public Sprite genericArtwork;

    public CardInShopWithPrice(
        CardType cardType,
        int price,
        CompanionTypeSO companionType,
        CardPoolSO cardPool,
        Card.CardRarity rarity,
        Sprite genericArtwork = null
    ) {
        this.cardType = cardType;
        this.price = price;
        this.sourceCompanion = companionType;
        this.cardPool = cardPool;
        this.rarity = rarity;
        this.genericArtwork = genericArtwork;
    }
}

[System.Serializable]
public class CompanionInShopWithPrice {
    public CompanionTypeSO companionType;
    public int price;

    public CompanionInShopWithPrice(CompanionTypeSO companionType, int price) {
        this.companionType = companionType;
        this.price = price;
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
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
    }

    public void Build(
            ShopManager shopManager,
            List<Companion> companionList,
            EncounterConstantsSO constants,
            ShopLevel shopLevel,
            bool USE_NEW_SHOP) {
        this.shopManager = shopManager;
        this.encounterConstants = constants;
        this.encounterType = EncounterType.Shop;
        // UIDocumentHoverableInstantiator.Instance.CleanupAllHoverables();s
        validateShopData();
        generateShopEncounter(shopLevel, companionList);
        if (USE_NEW_SHOP) {
            cardsInShop.ForEach(card => shopManager.shopViewController.AddCardToShopView(card));
            companionsInShop.ForEach(companion => shopManager.shopViewController.AddCompanionToShopView(companion));
        } else {
            setupCards();
            setupKeepsakes();
        }
        shopManager.SetupUnitManagement();
        // shopManager.shopViewController.SetupStaticHoverables();
    }

    public override void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder) {
        encounterBuilder.BuildShopEncounter(this);
    }

    private void setupCards() {
        for (int i = 0; i < cardsInShop.Count; i++) {
            GameObject instantiatedCard = GameObject.Instantiate(
                encounterConstants.cardInShopPrefab,
                this.shopManager.shopUIManager.cardSection
                );

            CardType cardType = cardsInShop[i].cardType;
            // Nullable for neutral cards, otherwise it will be attached to a specific companion type.
            CompanionTypeSO companionType = cardsInShop[i].sourceCompanion;

            CardInShop cardInShop = instantiatedCard.GetComponent<CardInShop>();
            cardInShop.price = cardsInShop[i].price;

            CardDisplay cardDisplay = cardInShop.cardDisplay;
            if (companionType != null) {
                cardInShop.keepSake.sprite = companionType.sprite;
            }
            //cardDisplay.Initialize(new Card(cardType, companionType, cardsInShop[i].rarity));

            cardInShop.Setup();
        }
    }

    private void setupKeepsakes() {
        for (int i = 0; i < companionsInShop.Count; i++) {
            GameObject instantiatedKeepsake = GameObject.Instantiate(
                encounterConstants.keepsakeInShopPrefab,
                this.shopManager.shopUIManager.keepSakeSection);

            CompanionTypeSO companionType = companionsInShop[i].companionType;
            int price = companionsInShop[i].price;

            KeepsakeInShop keepsakeInShop = instantiatedKeepsake.GetComponent<KeepsakeInShop>();
            keepsakeInShop.price = price;
            keepsakeInShop.companion = new Companion(companionType);
            keepsakeInShop.Setup();
        }
    }

    private void generateShopEncounter(ShopLevel shopLevel, List<Companion> companionList) {
        cardsInShop = new List<CardInShopWithPrice>();
        companionsInShop = new List<CompanionInShopWithPrice>();

        generateCards(shopLevel, companionList);
        generateKeepsakes(shopLevel, companionList);
    }

    private void generateCards(ShopLevel shopLevel, List<Companion> companionList) {
        ShopCardProbabilityDistBuilder b = new(
            shopData, shopLevel, companionList
        );
        ShopProbabilityDistribution dist = b.Build();
        dist.PrintCardDistribution();
        List<CardInShopWithPrice> chosen = dist.ChooseWithoutReplacement(shopLevel.numCardsToShow);
        foreach (CardInShopWithPrice z in chosen) {
            cardsInShop.Add(z);
        }
    }

    public void generateKeepsakes(ShopLevel shopLevel, List<Companion> team) {
        int numKeepsakesToGenerate = shopLevel.numKeepsakesToShow;

        // Maintain a list of the keepsakes that are out of the shop pool.
        // All companion types on your team are not considered in the pool.
        List<CompanionTypeSO> keepsakesOutOfPool = team.Select(x => x.companionType).ToList();

        for (int i = 0; i < numKeepsakesToGenerate; i++) {
            // Determine what the companion pool is for this single keepsake being generated
            Rarity rarity = PickRarity(
                shopLevel.commonCompanionPercentage,
                shopLevel.uncommonCompanionPercentage,
                shopLevel.rareCompanionPercentage,
                shopData.companionPool.commonCompanions.Count > 0,
                shopData.companionPool.uncommonCompanions.Count > 0,
                shopData.companionPool.rareCompanions.Count > 0
            );

            List<CompanionTypeSO> companions = new List<CompanionTypeSO>();
            switch(rarity) {
                case Rarity.Common:
                    companions = shopData.companionPool.commonCompanions;
                break;

                case Rarity.Uncommon:
                    companions = shopData.companionPool.uncommonCompanions;
                break;

                case Rarity.Rare:
                    companions = shopData.companionPool.rareCompanions;
                break;
            }

            // Simple weighted sampling algorithm: N copies of each companion type in the list,
            // where N corresponds to their weight.
            // Then we pick one uniformly at random.
            List<CompanionTypeSO> companionSampleDist = new();
            foreach (CompanionTypeSO c in companions) {
                // "Scarcity" mechanic; we reduce the number of companions
                // available by removing the keepsake count after pool.
                int numAvailable = shopData.numKeepsakeCopies - numCompanionsOfType(keepsakesOutOfPool, c);
                // in the case, where we exhaust all the companions of a given type, let there
                // be 1 available always, just so it is possible but much less likely.
                numAvailable = Math.Max(1, numAvailable);
                companionSampleDist.AddRange(Enumerable.Repeat(c, numAvailable));
                Debug.Log("Shop scarcity. " + c.name + ": " + numAvailable.ToString());
            }

            // Pick a keepsake from the sample distribution and add it to the shop's cards
            int number = UnityEngine.Random.Range(0, companionSampleDist.Count);
            CompanionTypeSO selected = companionSampleDist[number];
            companionsInShop.Add(new CompanionInShopWithPrice(selected, shopData.companionKeepsakePrice));
            keepsakesOutOfPool.Add(selected);
        }
    }

    private int numCompanionsOfType(List<CompanionTypeSO> companions, CompanionTypeSO companionType) {
        int count = 0;
        foreach (CompanionTypeSO c in companions) {
            if (c == companionType) {
                count++;
            }
            // If we have a combined version on the team, that effectively
            // means we bought 3 of the same kind.
            if (c == companionType.upgradeTo) {
                count += 3;
            }
        }
        return count;
    }

    private Rarity PickRarity(
            int commonPercent,
            int uncommonPercent,
            int rarePercent,
            bool commons,
            bool uncommons,
            bool rares) {
        List<Rarity> rarityPool = new List<Rarity>();
        List<int> percents = new List<int>();
        if (commons) {
            rarityPool.Add(Rarity.Common);
            percents.Add(commonPercent);
        }

        if (uncommons) {
            rarityPool.Add(Rarity.Uncommon);
            percents.Add(uncommonPercent);
        }

        if (rares) {
            rarityPool.Add(Rarity.Rare);
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
        return Rarity.Common;
    }

    private enum Rarity {
        Common,
        Uncommon,
        Rare
    }

    private void validateShopData(){
        List<CompanionTypeSO> allCompanions = new List<CompanionTypeSO>();
        allCompanions.AddRange(shopData.companionPool.commonCompanions);
        allCompanions.AddRange(shopData.companionPool.uncommonCompanions);
        allCompanions.AddRange(shopData.companionPool.rareCompanions);
        foreach(CompanionTypeSO companion in allCompanions) {
            if (companion.cardPool.commonCards.Count == 0 &&
                companion.cardPool.uncommonCards.Count == 0 &&
                companion.cardPool.rareCards.Count == 0) {
                    Debug.LogError("Companion " + companion.name + " has no cards in its pool. " +
                        "This will cause an error when generating a shop encounter.");
            }
        }
    }
}

[System.Serializable]
public class ShopEncounterSerializable : EncounterSerializable {
    public ShopEncounterSerializable(ShopEncounter encounter) : base(encounter) {
    }
}