using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopItemView : ICompanionViewDelegate {
    public VisualElement shopItemElement;
    public VisualElementFocusable visualElementFocusable;
    public CompanionInShopWithPrice companionInShop = null;
    public CardInShopWithPrice cardInShop = null;
    public BoosterPackInShopWithPrice boosterPackInShop = null;
    private CompanionView companionView = null;
    private IShopItemViewDelegate viewDelegate;
    private static string COIN = char.ConvertFromUtf32(0xE001);
    private bool interactionsEnabled = true;

    public ShopItemView(IShopItemViewDelegate viewDelegate, CompanionInShopWithPrice companion, VisualTreeAsset template = null) {
        this.viewDelegate = viewDelegate;
        shopItemElement = MakeCompanionShopItem(companion, template);
        companionInShop = companion;
    }

    public ShopItemView(IShopItemViewDelegate viewDelegate, CardInShopWithPrice card) {
        this.viewDelegate = viewDelegate;
        shopItemElement = MakeCardShopItem(card);
        cardInShop = card;
    }

    public ShopItemView(IShopItemViewDelegate viewDelegate, BoosterPackInShopWithPrice boosterPack) {
        this.viewDelegate = viewDelegate;
        shopItemElement = MakeBoosterPackShopItem(boosterPack);
        boosterPackInShop = boosterPack;
    }

    private VisualElement MakeCompanionShopItem(CompanionInShopWithPrice companion, VisualTreeAsset template = null) {
        VisualElement shopItemElement = new();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.AddToClassList("focusable");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();
        visualElementFocusable.SetInputAction(GFGInputAction.VIEW_DECK, DisplayCards);

        Companion tempCompanion = new(companion.companionType);
        tempCompanion.combatStats.IncreaseMaxHealth(companion.maxHealthBonus);
        tempCompanion.combatStats.currentHealth = tempCompanion.combatStats.getMaxHealth() - companion.sustainedDamage;
        for (int i = 0; i < ShopManager.Instance.GetShopLevel().numLessCardsInStartingDeck; i++) {
            tempCompanion.deck.PurgeStarterDeckCard(ShopManager.Instance.getShopEncounter().shopData.baseCardsToRemoveOnUpgrade);
        }

        companionView = new(tempCompanion, template, 0, CompanionView.SHOP_CONTEXT, this);
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

    private VisualElement MakeCardShopItem(CardInShopWithPrice card) {
        VisualElement shopItemElement = new();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.AddToClassList("focusable");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();

        CardView cardView = card.sourceCompanion != null
            ? new(card.cardType, card.sourceCompanion, card.rarity, true, card.packSO, true)
            : new(card.cardType, null, card.rarity, true, card.packSO, true);

        shopItemElement.Add(cardView.cardContainer);

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        shopItemElement.name = card.cardType.name;
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);

        shopItemElement.Add(CreatePriceTagForShopItem(card.price, card.increasedPrice));
        shopItemElement.Add(CreateCardSpawnIndicator(card));

        return shopItemElement;
    }

    private VisualElement MakeBoosterPackShopItem(BoosterPackInShopWithPrice boosterPack) {
        VisualElement shopItemElement = new();
        shopItemElement.AddToClassList("shop-item-container");
        shopItemElement.AddToClassList("focusable");
        shopItemElement.focusable = true;
        visualElementFocusable = shopItemElement.AsFocusable();

        // Create booster pack display here (adjust based on your BoosterPackView implementation)
        BoosterPackView boosterPackDisplay = new(boosterPack.boosterPack);
        shopItemElement.Add(boosterPackDisplay.GetContainer());

        shopItemElement.RegisterOnSelected(ShopItemViewOnClicked);
        shopItemElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        shopItemElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        shopItemElement.name = boosterPack.boosterPack.BoosterPackName();
        visualElementFocusable.additionalFocusAction += () => OnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += () => OnPointerLeave(null);

        shopItemElement.Add(CreatePriceTagForShopItem(boosterPack.price, boosterPack.increasedPrice));
        shopItemElement.Add(CreateCardSpawnIndicator(boosterPack.boosterPack));

        return shopItemElement;
    }

    private VisualElement CreateCardSpawnIndicator(CardInShopWithPrice card) {
        if (card.sourceCompanion == null && card.packSO == null) {
            return new VisualElement(); // Return an empty element if there's no source companion or pack
        }
        VisualElement cardSpawnIndicator = new();
        cardSpawnIndicator.AddToClassList("card_spawn_indicator_background");
        Color color = card.packSO != null ? card.packSO.packColor : card.sourceCompanion.pack.packColor;
        Color secondaryColor = card.packSO != null ? card.packSO.packSecondaryColor : card.sourceCompanion.pack.packSecondaryColor;
        cardSpawnIndicator.style.backgroundColor = new StyleColor(color);
        var c = new StyleColor(secondaryColor);
        cardSpawnIndicator.style.borderTopColor = c;
        cardSpawnIndicator.style.borderRightColor = c;
        cardSpawnIndicator.style.borderBottomColor = c;
        cardSpawnIndicator.style.borderLeftColor = c;

        VisualElement cardSpawnIndicatorIcon = new();
        cardSpawnIndicatorIcon.AddToClassList("card_spawn_indicator_icon");
        Sprite sprite = null;
        if (card.sourceCompanion != null) {
            Debug.Log("Card spawn indicator: " + card.sourceCompanion.name);
            sprite = card.sourceCompanion.icon;
        } else if (card.packSO != null) {
            Debug.Log("Card spawn indicator: " + card.packSO.name);
            sprite = card.packSO.packColorIcon;
        }
        cardSpawnIndicatorIcon.style.backgroundImage = new StyleBackground(sprite);
        cardSpawnIndicator.Add(cardSpawnIndicatorIcon);
        return cardSpawnIndicator;
    }

    private VisualElement CreateCardSpawnIndicator(BoosterPack boosterPack) {
        PackSO packSO = boosterPack.GetPackSO();
        if (packSO == null) {
            return new VisualElement(); // Return an empty element if there's no pack
        }
        VisualElement cardSpawnIndicator = new();
        cardSpawnIndicator.AddToClassList("card_spawn_indicator_background");
        cardSpawnIndicator.style.backgroundColor = new StyleColor(packSO.packColor);
        var c = new StyleColor(packSO.packSecondaryColor);
        cardSpawnIndicator.style.borderTopColor = c;
        cardSpawnIndicator.style.borderRightColor = c;
        cardSpawnIndicator.style.borderBottomColor = c;
        cardSpawnIndicator.style.borderLeftColor = c;

        VisualElement cardSpawnIndicatorIcon = new();
        cardSpawnIndicatorIcon.AddToClassList("card_spawn_indicator_icon");
        Sprite sprite = boosterPack is RatBoosterPackSO ratBoosterPack ? ratBoosterPack.ratSO.icon : packSO.packColorIcon;
        cardSpawnIndicatorIcon.style.backgroundImage = new StyleBackground(sprite);
        cardSpawnIndicator.Add(cardSpawnIndicatorIcon);
        return cardSpawnIndicator;
    }

    private VisualElement CreatePriceTagForShopItem(int price, bool increasedPrice = false) {
        VisualElement priceTag = new();
        Label label = new();
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
        } else if (cardInShop != null) {
            TooltipViewModel tvm = cardInShop.cardType.GetTooltip();
            if (!tvm.empty) {
                viewDelegate.DisplayTooltip(shopItemElement, tvm, TooltipContext.Shop);
            }
        } else if (boosterPackInShop != null) {
            viewDelegate.DisplayTooltip(shopItemElement, boosterPackInShop.boosterPack.GetTooltip(), TooltipContext.Shop);
        }
    }

    private void OnPointerLeave(PointerLeaveEvent evt) {
        if (!interactionsEnabled) return;
        viewDelegate.DestroyTooltip(shopItemElement);
    }

    private void DisplayCards() {
        viewDelegate.DisplayCards(companionInShop.companionType);
    }

    public Sprite GetStatusEffectSprite(StatusEffectType statusEffectType) => throw new System.NotImplementedException();

    public Sprite GetEnemyIntentImage(EnemyIntentType enemyIntentType) => throw new System.NotImplementedException();

    public CompanionView GetCompanionView() => companionView;

    public void ViewDeck(DeckViewType deckViewType, Companion companion = null, CompanionInstance companionInstance = null) => viewDelegate.DisplayCards(companionInShop.companionType);
}