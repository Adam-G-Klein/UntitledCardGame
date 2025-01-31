using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItemView {
    public VisualElement shopItemElement;
    public CompanionInShopWithPrice companionInShop = null;
    public CardInShopWithPrice cardInShop = null;

    private EntityView entityView = null;
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

    public void HideCompanionDescription() {
        if (entityView == null) {
            Debug.LogError("ShopItemView not setup to display a companion");
            return;
        }

        entityView.HideDescription();
    }

    private VisualElement makeCompanionShopItem(CompanionInShopWithPrice companion) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");

        // Bit of a hack, but I don't feel like completely refactoring entity view right now
        Companion tempCompanion = new Companion(companion.companionType);

        entityView = new EntityView(tempCompanion, 0, false);

        VisualElement portraitContainer = entityView.entityContainer.Q(className: "portrait-container");
        portraitContainer.style.backgroundImage = new StyleBackground(companion.companionType.sprite);

        shopItemElement.Add(entityView.entityContainer);

        shopItemElement.RegisterCallback<ClickEvent>(evt => ShopItemViewOnClicked());

        return shopItemElement;
    }

    private VisualElement makeCardShopItem(CardInShopWithPrice card) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");

        CardView cardView;
        if (card.sourceCompanion != null) {
            cardView = new CardView(card.cardType, card.sourceCompanion);
        } else {
            cardView = new CardView(card.cardType, card.genericArtwork);
        }

        shopItemElement.Add(cardView.cardContainer);

        shopItemElement.RegisterCallback<ClickEvent>(evt => ShopItemViewOnClicked());

        return shopItemElement;
    }

    private void ShopItemViewOnClicked() {
        viewDelegate.ShopItemOnClick(this);
    }
}