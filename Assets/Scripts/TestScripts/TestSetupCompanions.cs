using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestSetupCompanions : MonoBehaviour, IShopItemViewDelegate
{
    public GameStateVariableSO gameStateVariableSO;
    public UIDocument uiDoc;

    [ContextMenu("Go")]
    public void Go() {
        VisualElement mainArea = uiDoc.rootVisualElement.Q("shop-goods-area");
        foreach (Companion companion in gameStateVariableSO.companions.activeCompanions) {
            ShopItemView newShopItemView = new ShopItemView(this, new CompanionInShopWithPrice(companion.companionType, 1));
            mainArea.Add(newShopItemView.shopItemElement);
        }
        foreach (Companion companion in gameStateVariableSO.companions.activeCompanions) {
            ShopItemView newShopItemView = new ShopItemView(
                this, 
                new CardInShopWithPrice(
                    companion.deck.cards[0].cardType, 1, companion.companionType, Card.CardRarity.RARE)
            );
            mainArea.Add(newShopItemView.shopItemElement);
        }
    }

    public void RerollButtonOnClick()
    {
        throw new System.NotImplementedException();
    }

    public void ShopItemClickedOn(ShopItemView shopItemView)
    {
        Debug.Log("Thing was clicked on yippee!");
        Debug.Log(shopItemView.companionInShop);
        Debug.Log(shopItemView.cardInShop);
    }

    public void ShopItemOnClick(ShopItemView shopItemView)
    {
        throw new System.NotImplementedException();
    }

    public void UpgradeButtonOnClick()
    {
        throw new System.NotImplementedException();
    }

    public void ViewCompanionsOnClick()
    {
        throw new System.NotImplementedException();
    }
}
