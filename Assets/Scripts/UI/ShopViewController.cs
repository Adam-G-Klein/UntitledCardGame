using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopViewController : MonoBehaviour, IShopItemViewDelegate
{
    public UIDocument uiDoc;

    private ShopManager shopManager;
    private Dictionary<CardInShopWithPrice, ShopItemView> cardItemToViewMap;
    private Dictionary<CompanionInShopWithPrice, ShopItemView> companionItemToViewMap;

    // Specific shop VisualElement references
    private VisualElement shopGoodsArea;

    public void Init(ShopManager shopManager) {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }

        this.shopManager = shopManager;

        cardItemToViewMap = new Dictionary<CardInShopWithPrice, ShopItemView>();
        companionItemToViewMap = new Dictionary<CompanionInShopWithPrice, ShopItemView>();

        shopGoodsArea = uiDoc.rootVisualElement.Q("shop-goods-area");

        uiDoc.rootVisualElement.Q<Button>("reroll-button").clicked += RerollButtonOnClick;
        uiDoc.rootVisualElement.Q<Button>("upgrade-button").clicked += UpgradeButtonOnClick;
    }

    public void AddCardToShopView(CardInShopWithPrice card) {
        ShopItemView newCardItemView = new ShopItemView(this, card);

        shopGoodsArea.Add(newCardItemView.shopItemElement);

        cardItemToViewMap.Add(card, newCardItemView);
    }

    public void RemoveCardFromShopView(CardInShopWithPrice card) {
        ShopItemView shopItemView = cardItemToViewMap[card];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopGoodsArea.Remove(shopItemView.shopItemElement);
        
        cardItemToViewMap.Remove(card);
    }

    public void AddCompanionToShopView(CompanionInShopWithPrice companion) {
        ShopItemView newCompanionItemView = new ShopItemView(this, companion);

        shopGoodsArea.Add(newCompanionItemView.shopItemElement);

        companionItemToViewMap.Add(companion, newCompanionItemView);
    }

    public void RemoveCompanionFromShopView(CompanionInShopWithPrice companion) {
        ShopItemView shopItemView = companionItemToViewMap[companion];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopGoodsArea.Remove(shopItemView.shopItemElement);

        companionItemToViewMap.Remove(companion);
    }

    public void ShopItemOnClick(ShopItemView shopItemView) {
        if (shopItemView.companionInShop != null) {
            shopManager.ProcessCompanionBuyRequestV2(shopItemView, shopItemView.companionInShop);
        } else if (shopItemView.cardInShop != null) {
            shopManager.ProcessCardBuyRequestV2(shopItemView, shopItemView.cardInShop);
        }
    }

    public void RerollButtonOnClick() {
        shopManager.processRerollShopClick();
    }

    public void UpgradeButtonOnClick() {
        shopManager.processUpgradeShopClick();
    }
}
