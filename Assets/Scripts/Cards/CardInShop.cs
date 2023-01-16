using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using TMPro;

public class CardInShop : MonoBehaviour
    , IPointerClickHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    public int price;
    public string id;
    public CardBuyRequestEvent cardBuyEvent;

    public TMP_Text priceText;
    public CardDisplay cardDisplay;
    public Image hoverBackground;

    void Start() {
        Debug.Log("CardInShop Start() method");
        this.priceText.text = price.ToString();
        this.id = Id.newGuid();
        this.hoverBackground.enabled = false;
    }

    public void Setup() {
        Start();
    }

    public void OnPointerClick(PointerEventData eventData) {
        CardBuyRequest cardBuyRequest = new CardBuyRequest(
            cardDisplay.cardInfo, 
            price, 
            gameObject);
        cardBuyEvent.Raise(cardBuyRequest);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverBackground.enabled = true;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        hoverBackground.enabled = false;
    }

    public void shopRefreshEventHandler() {
        Debug.Log("Receive event");
        Destroy(this.gameObject);
    }
}
