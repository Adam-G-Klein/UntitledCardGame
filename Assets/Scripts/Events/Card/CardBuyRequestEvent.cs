using UnityEngine;

[System.Serializable]
public class CardBuyRequest {
    public Card cardInfo;
    public int price;
    public CardInShop cardInShop;

    public CardBuyRequest(Card cardInfo, int price, CardInShop cardInShop) {
        this.cardInfo = cardInfo;
        this.price = price;
        this.cardInShop = cardInShop;
    }
}

[CreateAssetMenu(
    fileName = "NewCardBuyRequestEvent", 
    menuName = "Events/Card/Card Buy Request Event")]
public class CardBuyRequestEvent : BaseGameEvent<CardBuyRequest> { }
