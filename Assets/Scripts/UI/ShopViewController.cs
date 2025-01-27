using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopViewController : MonoBehaviour, IShopItemViewDelegate
{
    public UIDocument uiDoc;
    public bool canDragCompanions = false;

    private ShopManager shopManager;
    private Dictionary<CardInShopWithPrice, ShopItemView> cardItemToViewMap;
    private Dictionary<CompanionInShopWithPrice, ShopItemView> companionItemToViewMap;

    // Specific shop VisualElement references
    private VisualElement shopGoodsArea;

    private bool alreadyScrolling = false;

    // For dragging and dropping companions in the unit management
    private bool isDraggingCompanion = false;
    private VisualElement companionBeingDragged = null;
    private VisualElement originalParent = null;

    public void Start() {
        Init(GetComponent<ShopManager>());
    }

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
        uiDoc.rootVisualElement.Q<Button>("start-next-combat-button").clicked += StartNextCombatOnClick;
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

    public void SetupUnitManagement(CompanionListVariableSO companionList) {
        SetupActiveCompanions(companionList.activeCompanions);
        SetupBenchCompanions(companionList.benchedCompanions);
    }

    public void SetupActiveCompanions(List<Companion> companions) {
        VisualElement activeContainer = uiDoc.rootVisualElement.Q("unit-active-container");
        for (int i = 0; i < companions.Count; i++) {
            VisualElement companionUI = CreateUnitManagementCompanion(companions[i]);
            activeContainer[i].Add(companionUI);
        }
    }

    public void SetupBenchCompanions(List<Companion> companions) {
        ScrollView benchContainer = uiDoc.rootVisualElement.Q<ScrollView>("bench-scroll-view");
        float fixedWidth = benchContainer.contentContainer[0].resolvedStyle.width;
        // By default, the UI contains slots for 5 bench companions
        // If we have more than 5, we'll need to programatically add more
        int slotsToAdd = companions.Count - 5;
        if (slotsToAdd > 0) {
            for (int i = 0; i < slotsToAdd; i++) {
                VisualElement newSlot = new VisualElement();
                newSlot.AddToClassList("single-unit-container");
                benchContainer.Add(newSlot);
            }
        }
        for (int i = 0; i < companions.Count; i++) {
            VisualElement companionUI = CreateUnitManagementCompanion(companions[i]);
            benchContainer.contentContainer[i].Add(companionUI);
        }

        // I can't fully figure out why this is necessary. Since we're using a scroll view,
        // as more items are added, if we use a fixed percent width for the items, then as the 
        // content visual element gets bigger, the items inside update to be a certain percent
        // of the new larger size. This sets them to the size they originally have in the base
        // UI document on scene start.
        for (int i = 0; i < benchContainer.contentContainer.childCount; i++) {
            benchContainer.contentContainer[i].style.width = new StyleLength(fixedWidth);
            Debug.Log(benchContainer.contentContainer[i].style.width);
        }
    }

    public VisualElement CreateUnitManagementCompanion(Companion companion) {
        EntityView entityView = new EntityView(companion, 0, false);
        entityView.SetupEntityImage(companion.companionType.sprite);
        entityView.HideDescription();
        entityView.entityContainer.RegisterCallback<ClickEvent>(evt => CompanionOnClick());
        entityView.entityContainer.RegisterCallback<PointerDownEvent>(CompanionOnPointerDown);
        entityView.entityContainer.RegisterCallback<PointerMoveEvent>(CompanionOnPointerMove);
        entityView.entityContainer.RegisterCallback<PointerUpEvent>(CompanionOnPointerUp);
        return entityView.entityContainer;
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

    public void StartNextCombatOnClick() {
        shopManager.exitShop();
    }

    public void CompanionOnClick() {
        Debug.Log("Companion clicked");
    }

    private void CompanionOnPointerDown(PointerDownEvent evt) {
        if (!canDragCompanions && !isDraggingCompanion) return;
        VisualElement target = evt.currentTarget as VisualElement;

        VisualElement parent = target.parent;

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parent.resolvedStyle.width * 1.2f;
        tempContainer.style.height = parent.resolvedStyle.height * 1.2f;
        tempContainer.style.position = Position.Absolute;

        uiDoc.rootVisualElement.Add(tempContainer);
        StartCoroutine(FinishDragSetup(tempContainer, target, evt.position, parent));
    }

    private IEnumerator FinishDragSetup(VisualElement tempContainer, VisualElement companion, Vector2 position, VisualElement originalSpot) {
        yield return new WaitForEndOfFrame();
        tempContainer.style.top = position.y - tempContainer.layout.height / 2;
        tempContainer.style.left = position.x - tempContainer.layout.width / 2;
        originalParent = originalSpot;
        originalSpot.Remove(companion);
        tempContainer.Add(companion);
        isDraggingCompanion = true;
        companionBeingDragged = companion;
    }

    private void CompanionOnPointerMove(PointerMoveEvent evt) {
        VisualElement target = evt.currentTarget as VisualElement;
        
        //Only take action if the player is dragging an item around the screen
        if (!isDraggingCompanion || companionBeingDragged != target)
        {
            return;
        }

        //Set the new position
        target.parent.style.top = evt.position.y - target.parent.layout.height / 2;
        target.parent.style.left = evt.position.x - target.parent.layout.width / 2;
    }

    private void CompanionOnPointerUp(PointerUpEvent evt) {
        VisualElement target = evt.currentTarget as VisualElement;
        if (!isDraggingCompanion || target != companionBeingDragged) return;
        VisualElement tempContainer = target.parent;
        originalParent.Add(target);
        uiDoc.rootVisualElement.Remove(tempContainer);
        isDraggingCompanion = false;
        companionBeingDragged = null;
        originalParent = null;
    }

    [ContextMenu("Test Scroll")]
    public void TestScroll() {
        ScrollView scrollView = uiDoc.rootVisualElement.Q<ScrollView>("bench-scroll-view");
        // Debug.Log(scrollView.childCount);
        // int children = scrollView.childCount;
        // float moveAmount = scrollView.contentRect.width / children;
        float moveAmount = scrollView.contentViewport.contentRect.width / 5;
        StartCoroutine(ScrollContent(scrollView, scrollView.scrollOffset.x, scrollView.scrollOffset.x + moveAmount));
    }

    private IEnumerator ScrollContent(ScrollView scrollView, float start, float end) {
        if (alreadyScrolling) {
            yield break;
        }
        alreadyScrolling = true;
        // TODO: fix spamming the button messing with where it starts / stops (just make sure no coroutine is running when you click button)
        float elapsedTime = 0f;
        float waitTime = 0.35f;
        while (elapsedTime < waitTime) {
            scrollView.scrollOffset = new Vector2(Mathf.Lerp(start, end, elapsedTime / waitTime), scrollView.scrollOffset.y);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        scrollView.scrollOffset = new Vector2(end, scrollView.scrollOffset.y);
        alreadyScrolling = false;
    }
}
