using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInShopWithPrice {
    public CardType cardType;
    public int price;

    // Nullable for neutral cards.
    public CompanionTypeSO sourceCompanion;

    public CardInShopWithPrice(CardType cardType, int price) {
        this.cardType = cardType;
        this.price = price;
        this.sourceCompanion = null;
    }

    public CardInShopWithPrice(CardType cardType, int price, CompanionTypeSO companionType) {
        this.cardType = cardType;
        this.price = price;
        this.sourceCompanion = companionType;
    }
}

[System.Serializable]
public class KeepsakeInShopWithPrice {
    public CompanionTypeSO companionType;
    public int price;

    public KeepsakeInShopWithPrice(CompanionTypeSO companionType, int price) {
        this.companionType = companionType;
        this.price = price;
    }
}

[System.Serializable]
public class ShopEncounter : Encounter
{
    public ShopDataSO shopData;
    public List<CardInShopWithPrice> cardsInShop = new List<CardInShopWithPrice>();
    public List<KeepsakeInShopWithPrice> keepsakesInShop = new List<KeepsakeInShopWithPrice>();
    private EncounterConstantsSO encounterConstants;

    private ShopManager shopManager;

    public ShopEncounter() {
        this.encounterType = EncounterType.Shop;
    }

    public ShopEncounter(ShopDataSO shopData) {
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
    }

    public void Build(
            ShopManager shopManager,
            List<Companion> companionList,
            EncounterConstantsSO constants,
            ShopLevel shopLevel) {
        this.shopManager = shopManager;
        this.encounterConstants = constants;
        this.encounterType = EncounterType.Shop;
        validateShopData();
        generateShopEncounter(shopLevel, companionList);
        setupCards();
        setupKeepsakes();
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

            // This structuring is a piece of garbage.
            // The same Script is used in two places; one on the parent `CardInShopHolder`
            // and one in the child `CardInShop` gameobject.
            // The `instantiatedCard` object is a CardInShopHolder prefab.
            // Its CardInShop script component is used to control the display.
            // The poorly named CardInShop gameobject intercepts click events and processes
            // buy requests - all we have to do for it is set the price.
            CardInShop cardInShop = instantiatedCard.GetComponent<CardInShop>();
            cardInShop.price = cardsInShop[i].price;

            CardDisplay cardDisplay = cardInShop.cardDisplay;
            if (companionType != null) {
                cardInShop.keepSake.sprite = companionType.keepsake;
            }
            cardDisplay.Initialize(new Card(cardType, companionType));

            cardInShop.Setup();

            CardInShop childCardInShop = instantiatedCard.transform.GetChild(0).GetComponent<CardInShop>();
            childCardInShop.price = cardsInShop[i].price;
            childCardInShop.Setup();
        }
    }

    private void setupKeepsakes() {
        for (int i = 0; i < keepsakesInShop.Count; i++) {
            GameObject instantiatedKeepsake = GameObject.Instantiate(
                encounterConstants.keepsakeInShopPrefab,
                this.shopManager.shopUIManager.keepSakeSection);

            CompanionTypeSO companionType = keepsakesInShop[i].companionType;
            int price = keepsakesInShop[i].price;

            KeepsakeInShop keepsakeInShop = instantiatedKeepsake.GetComponent<KeepsakeInShop>();
            keepsakeInShop.price = price;
            keepsakeInShop.companion = new Companion(companionType);
            keepsakeInShop.Setup();
        }
    }

    private void generateShopEncounter(ShopLevel shopLevel, List<Companion> companionList) {
        cardsInShop = new List<CardInShopWithPrice>();
        keepsakesInShop = new List<KeepsakeInShopWithPrice>();

        generateCards(shopLevel, companionList);
        generateKeepsakes(shopLevel);
    }

    private void generateCards(ShopLevel shopLevel, List<Companion> companionList) {
        //determine which companion to spawn a card from, remove them from the set
        //move companion types to a hashSet
        HashSet<CompanionTypeSO> companionTypes = new();
        foreach (Companion companion in companionList) {
            companionTypes.Add(companion.companionType);
        }
        List<CardInShopWithPrice> commonShopCards = new();
        List<CardInShopWithPrice> uncommonShopCards = new();
        List<CardInShopWithPrice> rareShopCards = new();
        // Add the card pools for each companion that is on your team.
        // We do not want to show cards for companions that you do not have.
        foreach (CompanionTypeSO companionType in companionTypes) {
            foreach (CardType card in companionType.cardPool.commonCards) {
                commonShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice, companionType));
            }
            foreach (CardType card in companionType.cardPool.uncommonCards) {
                uncommonShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice, companionType));
            }
            foreach (CardType card in companionType.cardPool.rareCards) {
                rareShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice, companionType));
            }
        }
        // Add the neutral cards to each card pool; note, they are not attached to a
        // specific companion.
        foreach (CardType card in shopData.neutralCardPool.commonCards) {
            commonShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice));
        }
        foreach (CardType card in shopData.neutralCardPool.uncommonCards) {
            uncommonShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice));
        }
        foreach (CardType card in shopData.neutralCardPool.rareCards) {
            rareShopCards.Add(new CardInShopWithPrice(card, shopData.cardPrice));
        }

        for (int i = 0; i < shopLevel.numCardsToShow; i++) {
            Rarity rarity = PickRarity(
                shopLevel.commonCardPercentage,
                shopLevel.uncommonCardPercentage,
                shopLevel.rareCardPercentage,
                commonShopCards.Count > 0,
                uncommonShopCards.Count > 0,
                rareShopCards.Count > 0
            );

            // Determine what the card pool is for this single card being generated
            List<CardInShopWithPrice> finalShopCardsPool = new();
            switch (rarity) {
                case Rarity.Common:
                    finalShopCardsPool = commonShopCards;
                break;

                case Rarity.Uncommon:
                    finalShopCardsPool = uncommonShopCards;
                break;

                case Rarity.Rare:
                    finalShopCardsPool = rareShopCards;
                break;
            }

            int selectedCardIndex = Random.Range(0, finalShopCardsPool.Count);
            CardInShopWithPrice selectedCard = finalShopCardsPool[selectedCardIndex];

            cardsInShop.Add(selectedCard);
        }
    }

    public void generateKeepsakes(ShopLevel shopLevel) {
        int numKeepsakesToGenerate = shopLevel.numKeepsakesToShow;

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
            int keepsakePrice = 0;
            switch(rarity) {
                case Rarity.Common:
                    companions = shopData.companionPool.commonCompanions;
                    keepsakePrice = shopData.companionPool.commonCompanionPrice;
                break;

                case Rarity.Uncommon:
                    companions = shopData.companionPool.uncommonCompanions;
                    keepsakePrice = shopData.companionPool.uncommonCompanionPrice;
                break;

                case Rarity.Rare:
                    companions = shopData.companionPool.rareCompanions;
                    keepsakePrice = shopData.companionPool.rareCompanionPrice;
                break;
            }

            // Pick a card from the pool and add it to the shop's cards
            int number = Random.Range(0, companions.Count);
            keepsakesInShop.Add(
                new KeepsakeInShopWithPrice(companions[number], keepsakePrice));
        }
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

        int randomNumber = Random.Range(0, totalPercentage); // min inclusive, max exclusive
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
