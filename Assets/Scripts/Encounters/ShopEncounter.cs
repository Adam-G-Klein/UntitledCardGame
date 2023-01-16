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
public class ShopEncounter : Encounter
{
    public bool generateEncounter;
    public ShopDataSO shopData;
    public List<CardInShopWithPrice> cardsInShop;

    private EncounterConstants encounterConstants;

    public ShopEncounter() {
        this.encounterType = EncounterType.Shop;
    }


    public override void build(EncounterConstants constants)
    {
        this.encounterType = EncounterType.Shop;
        this.encounterConstants = constants;
        if (generateEncounter) {
            generateShopEncounter();
        }
        setupCards();
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
            // cardInShop.Setup();
        }
    }

    private void generateShopEncounter() {
        // Since we're generating the shop, lets clear the current cards in shop
        cardsInShop = new List<CardInShopWithPrice>();

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
}
