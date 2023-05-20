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

    private ShopUIManager shopUIManager;

    public ShopEncounter() {
        this.encounterType = EncounterType.Shop;
    }

    public ShopEncounter(ShopDataSO shopData) {
        this.encounterType = EncounterType.Shop;
        this.shopData = shopData;
    }

    public override void build(List<Companion> companionList, EncounterConstantsSO constants)
    {
        if (shopUIManager == default) {
            findShopSection();
        }

        this.encounterType = EncounterType.Shop;
        ShopManager.Instance.saveShopEncounter(this);
        this.encounterConstants = constants;
        
        generateShopEncounter(companionList);
        setupCards(companionList);
        setupKeepsakes();
        this.shopData.shopEncounterEvent.Raise(this);
    }

    private void findShopSection() {
        shopUIManager = GameObject.FindObjectOfType<ShopUIManager>();

        Debug.Assert(shopUIManager, "Unable to find shope UI manager, required to set up the cards and keepsakes for sale!");
    }

    private void setupCards(List<Companion> companionList) {        
        for (int i = 0; i < cardsInShop.Count; i++) {
            GameObject instantiatedCard = GameObject.Instantiate(
                encounterConstants.cardInShopPrefab, 
                shopUIManager.cardSection
                );

            CardType cardType = cardsInShop[i].cardType;
            int price = cardsInShop[i].price;

            CardInShop cardInShop = instantiatedCard.GetComponent<CardInShop>();
            cardInShop.price = price;

            CardDisplay cardDisplay = cardInShop.cardDisplay;
            cardDisplay.cardInfo = new Card(cardType);

            //NOTE: Assumes that the companion list and the order their cards are displayed are the same
            cardInShop.keepSake.sprite = companionList[i].companionType.keepsake;
        }
    }

    private void setupKeepsakes() {
        for (int i = 0; i < keepsakesInShop.Count; i++) {
            GameObject instantiatedKeepsake = GameObject.Instantiate(
                encounterConstants.keepsakeInShopPrefab, 
                shopUIManager.keepSakeSection);

            CompanionTypeSO companionType = keepsakesInShop[i].companionType;
            int price = keepsakesInShop[i].price;

            KeepsakeInShop keepsakeInShop = instantiatedKeepsake.GetComponent<KeepsakeInShop>();
            keepsakeInShop.price = price;
            keepsakeInShop.companion = new Companion(companionType);
            keepsakeInShop.Setup();
        }
    }

    private void generateShopEncounter(List<Companion> companionList) {
        cardsInShop = new List<CardInShopWithPrice>();
        keepsakesInShop = new List<KeepsakeInShopWithPrice>();

        generateCards(companionList);
        generateKeepsakes();
    }

    private void generateCards(List<Companion> companionList) {
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
            int totalPercentage = currentCardPool.getTotalCardPercentage();
            int commonCardPercentage = currentCardPool.commonCardPercentage;
            int uncommonCardPercentage = currentCardPool.uncommonCardPercentage;
            int rareCardPercentage = currentCardPool.rareCardPercentage;


            // Determine what the card pool is for this single card being generated
            SerializableHashSet<CardType> cardSet = new SerializableHashSet<CardType>();
            int cardPrice = 0;
            int randomNumber = Random.Range(0, totalPercentage); // min inclusive, max exclusive
            if (randomNumber < commonCardPercentage) {
                cardSet = currentCardPool.commonCards;
                cardPrice = currentCardPool.commonCardPrice;
            } else if (randomNumber < commonCardPercentage + uncommonCardPercentage) {
                cardSet = currentCardPool.uncommonCards;
                cardPrice = currentCardPool.uncommonCardPrice;
            } else {
                cardSet = currentCardPool.rareCards;
                cardPrice = currentCardPool.rareCardPrice;
            }

            // Pick a card from the pool and add it to the shop's cards
            int cardNumber = Random.Range(0, cardSet.Count);

            //super cool and efficent random selection from a hashSet(it has to iterate through the collection, there is no index in a hashset)
            CardType selectedCard = default;
            int i = 0;
            foreach (CardType card in cardSet) {
                if (i == cardNumber) {
                    selectedCard = card;
                    break;
                }
                i++;
            }

            cardsInShop.Add(new CardInShopWithPrice(selectedCard, cardPrice));
        }

    }

    public void generateKeepsakes() {
        int numKeepsakesToGenerate = shopData.keepsakeCount;
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
