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
    public bool generateEncounter;
    public ShopDataSO shopData;
    public List<CardInShopWithPrice> cardsInShop = new List<CardInShopWithPrice>();
    public List<KeepsakeInShopWithPrice> keepsakesInShop = new List<KeepsakeInShopWithPrice>();
    private EncounterConstantsSO encounterConstants;

    public ShopEncounter() {
        this.encounterType = EncounterType.Shop;
    }

    public ShopEncounter(ShopDataSO shopData) {
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
        generateShopEncounter();
    }

    public override void build(List<Companion> companionList, EncounterConstantsSO constants)
    {
        this.encounterType = EncounterType.Shop;
        this.encounterConstants = constants;
        if (generateEncounter) {
            generateShopEncounter();
        }
        setupCards();
        setupKeepsakes();
    }

    private void setupCards() {
        if (cardsInShop.Count > shopData.cardLocations.Count) {
            Debug.LogError("Unable to setup cards in shop encounter, not enough locations for cards");
            return;
        }
        
        for (int i = 0; i < cardsInShop.Count; i++) {
            GameObject instantiatedCard = GameObject.Instantiate(
                encounterConstants.cardInShopPrefab, 
                shopData.cardLocations[i],
                Quaternion.identity);

            CardType cardType = cardsInShop[i].cardType;
            int price = cardsInShop[i].price;

            CardDisplay cardDisplay = instantiatedCard.GetComponent<CardDisplay>();
            cardDisplay.cardInfo = new Card(cardType);

            CardInShop cardInShop = instantiatedCard.GetComponent<CardInShop>();
            cardInShop.price = price;
        }
    }

    private void setupKeepsakes() {
        if (keepsakesInShop.Count > shopData.keepsakeLocations.Count) {
            Debug.LogError("Unable to setup keepsakes in shop encounter, not enough locations for keepsakes");
            return;
        }

        for (int i = 0; i < keepsakesInShop.Count; i++) {
            GameObject instantiatedKeepsake = GameObject.Instantiate(
                encounterConstants.keepsakeInShopPrefab, 
                shopData.keepsakeLocations[i],
                Quaternion.identity);

            CompanionTypeSO companionType = keepsakesInShop[i].companionType;
            int price = keepsakesInShop[i].price;

            KeepsakeInShop keepsakeInShop = instantiatedKeepsake.GetComponent<KeepsakeInShop>();
            keepsakeInShop.price = price;
            keepsakeInShop.companion = new Companion(companionType);
            keepsakeInShop.Setup();
        }
    }

    private void generateShopEncounter() {
        // Since we're generating the shop, lets clear the current shop
        cardsInShop = new List<CardInShopWithPrice>();
        keepsakesInShop = new List<KeepsakeInShopWithPrice>();

        generateCards();
        generateKeepsakes();
    }

    private void generateCards() {
        int numCardsToGenerate = shopData.cardLocations.Count;
        int totalPercentage = shopData.getTotalCardPercentage();
        int commonCardPercentage = shopData.commonCardPercentage;
        int uncommonCardPercentage = shopData.uncommonCardPercentage;
        int rareCardPercentage = shopData.rareCardPercentage;

        for (int i = 0; i < numCardsToGenerate; i++) {
            // Determine what the card pool is for this single card being generated
            List<CardType> cardPool = new List<CardType>();
            int cardPrice = 0;
            int randomNumber = Random.Range(0, totalPercentage); // min inclusive, max exclusive
            if (randomNumber < commonCardPercentage) {
                cardPool = shopData.commonCards;
                cardPrice = shopData.commonCardPrice;
            } else if (randomNumber < commonCardPercentage + uncommonCardPercentage) {
                cardPool = shopData.uncommonCards;
                cardPrice = shopData.uncommonCardPrice;
            } else {
                cardPool = shopData.rareCards;
                cardPrice = shopData.rareCardPrice;
            }

            // Pick a card from the pool and add it to the shop's cards
            int cardNumber = Random.Range(0, cardPool.Count);
            cardsInShop.Add(
                new CardInShopWithPrice(cardPool[cardNumber], cardPrice));
        }
    }

    public void generateKeepsakes() {
        int numKeepsakesToGenerate = shopData.keepsakeLocations.Count;
        int totalPercentage = shopData.getTotalCompanionPercentage();
        int commonPercentage = shopData.commonCompanionPercentage;
        int uncommonPercentage = shopData.uncommonCompanionPercentage;
        int rarePercentage = shopData.rareCompanionPercentage;

        for (int i = 0; i < numKeepsakesToGenerate; i++) {
            // Determine what the companion pool is for this single keepsake being generated
            List<CompanionTypeSO> companionPool = new List<CompanionTypeSO>();
            int keepsakePrice = 0;
            int randomNumber = Random.Range(0, totalPercentage); // min inclusive, max exclusive
            if (randomNumber < commonPercentage) {
                companionPool = shopData.commonCompanions;
                keepsakePrice = shopData.commonCompanionPrice;
            } else if (randomNumber < commonPercentage + uncommonPercentage) {
                companionPool = shopData.uncommonCompanions;
                keepsakePrice = shopData.uncommonCompanionPrice;
            } else {
                companionPool = shopData.rareCompanions;
                keepsakePrice = shopData.rareCompanionPrice;
            }

            // Pick a card from the pool and add it to the shop's cards
            int number = Random.Range(0, companionPool.Count);
            keepsakesInShop.Add(
                new KeepsakeInShopWithPrice(companionPool[number], keepsakePrice));
        }
    }
}
