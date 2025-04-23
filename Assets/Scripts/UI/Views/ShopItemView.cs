using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItemView {
    public VisualElement shopItemElement;
    public VisualElementFocusable visualElementFocusable;
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
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();

        // Bit of a hack, but I don't feel like completely refactoring entity view right now
        Companion tempCompanion = new Companion(companion.companionType);

        entityView = new EntityView(tempCompanion, 0, false);
        entityView.UpdateWidthAndHeight();

        VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
        portraitContainer.style.backgroundImage = new StyleBackground(companion.companionType.sprite);

        shopItemElement.Add(entityView.entityContainer);

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);
        shopItemElement.name = companion.companionType.name;

        shopItemElement.Add(CreatePriceTagForShopItem(companion.price));

        return shopItemElement;
    }

    private VisualElement makeCardShopItem(CardInShopWithPrice card) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();

        CardView cardView;
        if (card.sourceCompanion != null) {
            cardView = new CardView(card.cardType, card.sourceCompanion, card.rarity, true);
        } else {
            cardView = new CardView(card.cardType, card.genericArtwork, card.rarity, true);
        }

        shopItemElement.Add(cardView.cardContainer);

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        shopItemElement.name = card.cardType.name;
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);
        // UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(shopItemElement,
        //     ShopItemViewOnClicked,
        //     ()=> {OnPointerEnter(null);},
        //     () => {OnPointerLeave(null);});

        shopItemElement.Add(CreatePriceTagForShopItem(card.price));

        return shopItemElement;
    }

    private VisualElement CreatePriceTagForShopItem(int price) {
        VisualElement priceTag = new VisualElement();
        Label label = new Label();
        priceTag.AddToClassList("shop-item-price-tag-background");
        label.AddToClassList("shop-item-price-tag-label");
        label.text = "$" + price.ToString();
        priceTag.Add(label);
        return priceTag;
    }

    private void ShopItemViewOnClicked() {
        viewDelegate.ShopItemOnClick(this);
        viewDelegate.DestroyTooltip(shopItemElement);
    }

    public void Disable() {
        shopItemElement.visible = false;
    }

    private void OnPointerEnter(PointerEnterEvent evt) {
        viewDelegate.ShopItemViewHovered(this);
        if (companionInShop != null) {
            viewDelegate.DisplayTooltip(shopItemElement, companionInShop.companionType.tooltip, false);
        } else {
            TooltipViewModel tvm = cardInShop.cardType.GetTooltip();
            if (tvm.empty) return;
            viewDelegate.DisplayTooltip(shopItemElement, tvm, false);
        }
    }
    private void OnPointerLeave(PointerLeaveEvent evt) {
             viewDelegate.DestroyTooltip(shopItemElement);
    }
}