using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInShopWithPrice {
    public CardType cardType;
    public int price;

    public CardInShopWithPrice(CardType cardType, int price) {
        this.cardType = cardType;
        this.price = price;
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
        setupCards(companionList);
        setupKeepsakes();
    }

    public override void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder) {
        encounterBuilder.BuildShopEncounter(this);
    }

    private void setupCards(List<Companion> companionList) {        
        for (int i = 0; i < cardsInShop.Count; i++) {
            GameObject instantiatedCard = GameObject.Instantiate(
                encounterConstants.cardInShopPrefab, 
                this.shopManager.shopUIManager.cardSection
                );

            CardType cardType = cardsInShop[i].cardType;
            int price = cardsInShop[i].price;

            CardInShop cardInShop = instantiatedCard.GetComponent<CardInShop>();
            cardInShop.price = price;

            CardDisplay cardDisplay = cardInShop.cardDisplay;
            CompanionTypeSO companionType = companionList[i].companionType;
            cardInShop.keepSake.sprite = companionType.keepsake;
            cardDisplay.Initialize(new Card(cardType, companionType));

            // NOTE: Assumes that the companion list and the order their cards are displayed are the same

            cardInShop.Setup();
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
        Debug.Log("Companion List count: " + companionList.Count);
        //determine which companion to spawn a card from, remove them from the set
        //move companion types to a hashSet
        HashSet<CompanionTypeSO> companionTypes = new HashSet<CompanionTypeSO>();

        foreach (Companion companion in companionList) {
            companionTypes.Add(companion.companionType);
        }

        Debug.Log("Companion types: " + companionTypes.Count);

        foreach (CompanionTypeSO companionType in companionTypes) {
            //pick a random card based on random algorithm
            CardPoolSO currentCardPool = companionType.cardPool;
            Rarity rarity = PickRarity(
                shopLevel.commonCardPercentage,
                shopLevel.uncommonCardPercentage,
                shopLevel.rareCardPercentage,
                currentCardPool.commonCards.Count > 0,
                currentCardPool.uncommonCards.Count > 0,
                currentCardPool.rareCards.Count > 0
            );

            // Determine what the card pool is for this single card being generated
            List<CardType> cardSet = new List<CardType>();
            int cardPrice = 0;
            switch (rarity) {
                case Rarity.Common:
                    cardSet = currentCardPool.commonCards;
                    cardPrice = currentCardPool.commonCardPrice;
                break;

                case Rarity.Uncommon:
                    cardSet = currentCardPool.uncommonCards;
                    cardPrice = currentCardPool.uncommonCardPrice;
                break;

                case Rarity.Rare:
                    cardSet = currentCardPool.rareCards;
                    cardPrice = currentCardPool.rareCardPrice;
                break;
            }

            // Pick a card from the pool and add it to the shop's cards
            int cardNumber = Random.Range(0, cardSet.Count);

            //super cool and efficent random selection from a hashSet(it has to iterate through the collection, there is no index in a hashset)
            CardType selectedCard = cardSet[cardNumber];
            cardsInShop.Add(new CardInShopWithPrice(selectedCard, cardPrice));
        }

    }

    public void generateKeepsakes(ShopLevel shopLevel) {
        int numKeepsakesToGenerate = shopData.keepsakeCount;

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
