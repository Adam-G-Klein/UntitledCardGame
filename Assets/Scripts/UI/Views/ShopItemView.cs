using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItemView {
    public VisualElement shopItemElement;
    public CompanionInShopWithPrice companionInShop = null;
    public CardInShopWithPrice cardInShop = null;

    private float SCREEN_WIDTH_PERCENT = 0.125f;
    private float RATIO = 1.6f;

    private IShopItemViewDelegate viewDelegate;


    public ShopItemView(IShopItemViewDelegate viewDelegate, CompanionInShopWithPrice companion) {
        this.viewDelegate = viewDelegate;
        shopItemElement = makeCompanionShopItem(companion);
        companionInShop = companion;
    }

    public ShopItemView(IShopItemViewDelegate viewDelegate, CardInShopWithPrice card) {
        this.viewDelegate = viewDelegate;
        shopItemElement = makeCardShopItem(card);
        cardInShop = card;
    }

    private VisualElement makeCompanionShopItem(CompanionInShopWithPrice companion) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");
        Debug.Log(Camera.main);
        Debug.Log(Camera.main.pixelWidth);
        int width = (int)(Camera.main.pixelWidth * SCREEN_WIDTH_PERCENT);
        int height = (int)(width * RATIO);
        shopItemElement.style.width = width;
        shopItemElement.style.height = height;


        // Bit of a hack, but I don't feel like completely refactoring entity view right now
        Companion tempCompanion = new Companion(companion.companionType);

        EntityView entityView = new EntityView(tempCompanion, 0, false);

        VisualElement portraitContainer = entityView.entityContainer.Q(className: "portrait-container");
        portraitContainer.style.backgroundImage = new StyleBackground(companion.companionType.sprite);

        shopItemElement.Add(entityView.entityContainer);

        shopItemElement.RegisterCallback<ClickEvent>(evt => ShopItemViewOnClicked());

        return shopItemElement;
    }

    private VisualElement makeCardShopItem(CardInShopWithPrice card) {
        // TODO
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");

        CardView cardView = new CardView(card.cardType, card.sourceCompanion);

        shopItemElement.Add(cardView.cardContainer);

        shopItemElement.RegisterCallback<ClickEvent>(evt => ShopItemViewOnClicked());

        return shopItemElement;
    }

    private void ShopItemViewOnClicked() {
        viewDelegate.ShopItemClickedOn(this);
    }
}