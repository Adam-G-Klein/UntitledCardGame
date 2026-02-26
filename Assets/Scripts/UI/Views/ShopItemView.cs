using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItemView : ICompanionViewDelegate {
    public VisualElement shopItemElement;
    public VisualElementFocusable visualElementFocusable;
    public CompanionInShopWithPrice companionInShop = null;
    public CardInShopWithPrice cardInShop = null;
    private CompanionView companionView = null;
    private IShopItemViewDelegate viewDelegate;
    private static string COIN = char.ConvertFromUtf32(0xE001);
    
    private bool interactionsEnabled = true;

    public ShopItemView(IShopItemViewDelegate viewDelegate, CompanionInShopWithPrice companion, VisualTreeAsset template = null) {
        this.viewDelegate = viewDelegate;
        shopItemElement = makeCompanionShopItem(companion, template);
        companionInShop = companion;
    }

    public ShopItemView(IShopItemViewDelegate viewDelegate, CardInShopWithPrice card) {
        this.viewDelegate = viewDelegate;
        shopItemElement = makeCardShopItem(card);
        cardInShop = card;
    }

    private VisualElement makeCompanionShopItem(CompanionInShopWithPrice companion, VisualTreeAsset template = null) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.AddToClassList("focusable");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();
        visualElementFocusable.SetInputAction(GFGInputAction.VIEW_DECK, DisplayCards);

        // Bit of a hack, but I don't feel like completely refactoring entity view right now
        Companion tempCompanion = new Companion(companion.companionType);
        tempCompanion.combatStats.IncreaseMaxHealth(companion.maxHealthBonus);
        tempCompanion.combatStats.currentHealth = tempCompanion.combatStats.getMaxHealth() - companion.sustainedDamage;
        for (int i = 0; i < ShopManager.Instance.GetShopLevel().numLessCardsInStartingDeck; i++) {
            tempCompanion.deck.PurgeStarterDeckCard(ShopManager.Instance.gameState.baseShopData.baseCardsToRemoveOnUpgrade);
        }
        // for (int i = 0; i < tempCompanion.deck.cards.Count; i++) {
            // Debug.Log("Card at index " + i + " in temp companion deck: " + tempCompanion.deck.cards[i].cardType.name);
        // }

        companionView = new CompanionView(tempCompanion, template, 0, CompanionView.SHOP_CONTEXT, this);
        shopItemElement.Add(companionView.container);

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalFocusAction += () => companionView.HoverDetectorPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);
        visualElementFocusable.additionalUnfocusAction += () => companionView.HoverDetectorPointerLeave(null);
        shopItemElement.name = companion.companionType.name;

        shopItemElement.Add(CreatePriceTagForShopItem(companion.price, companion.increasedPrice));

        return shopItemElement;
    }

    private VisualElement makeCardShopItem(CardInShopWithPrice card) {
        VisualElement shopItemElement = new VisualElement();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.AddToClassList("focusable");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();

        CardView cardView;
        if (card.sourceCompanion != null) {
            cardView = new CardView(card.cardType, card.sourceCompanion, card.rarity, true, card.packSO);
        } else {
            cardView = new CardView(card.cardType, null, card.rarity, true, card.packSO);
        }

        shopItemElement.Add(cardView.cardContainer);

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        shopItemElement.name = card.cardType.name;
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);

        shopItemElement.Add(CreatePriceTagForShopItem(card.price, card.increasedPrice));

        return shopItemElement;
    }

    private VisualElement CreatePriceTagForShopItem(int price, bool increasedPrice = false) {
        VisualElement priceTag = new VisualElement();
        Label label = new Label();
        priceTag.AddToClassList("shop-item-price-tag-background");
        label.AddToClassList("shop-item-price-tag-label");
        label.text = COIN + price.ToString();
        priceTag.Add(label);

        if (increasedPrice) {
            label.AddToClassList("shop-item-price-tag-increased");
        }
        return priceTag;
    }

    private void ShopItemViewOnClicked() {
        if (!interactionsEnabled) return;

        viewDelegate.ShopItemOnClick(this);
        viewDelegate.DestroyTooltip(shopItemElement);
    }

    public void Disable() {
        shopItemElement.visible = false;
    }

    public void DisableInteractions() {
        interactionsEnabled = false;
    }

    public void EnableInteractions() {
        interactionsEnabled = true;
    }

    private void OnPointerEnter(PointerEnterEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate.ShopItemViewHovered(this);
        if (companionInShop != null) {
            viewDelegate.DisplayTooltip(shopItemElement, companionInShop.companionType.GetTooltip(), TooltipContext.Shop);
        } else {
            TooltipViewModel tvm = cardInShop.cardType.GetTooltip();
            if (tvm.empty) return;
            viewDelegate.DisplayTooltip(shopItemElement, tvm, TooltipContext.Shop);
        }
    }

    private void OnPointerLeave(PointerLeaveEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate.DestroyTooltip(shopItemElement);
    }

    private void DisplayCards() {
        viewDelegate.DisplayCards(companionInShop.companionType);
    }

    public Sprite GetStatusEffectSprite(StatusEffectType statusEffectType)
    {
        throw new NotImplementedException();
    }

    public Sprite GetEnemyIntentImage(EnemyIntentType enemyIntentType)
    {
        throw new NotImplementedException();
    }

    public CompanionView GetCompanionView()
    {
        return companionView;
    }

    public void ViewDeck(DeckViewType deckViewType, Companion companion = null, CompanionInstance companionInstance = null)
    {
        viewDelegate.DisplayCards(companionInShop.companionType);
    }
}