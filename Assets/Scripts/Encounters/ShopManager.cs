using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardBuyRequestEventListener))]
public class ShopManager : MonoBehaviour
{
    public ShopUIManager shopUIManager;
    public PlayerDataReference playerData;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void processBuyRequest(CardBuyRequest cardBuyRequest) {
        if (playerData.Value.gold >= cardBuyRequest.price) {
            // Add the card to the minion (how decide what minion???)
            Debug.Log("Card (not) added to minion");
            playerData.Value.gold -= cardBuyRequest.price;
            Destroy(cardBuyRequest.cardInShop);
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }
}
