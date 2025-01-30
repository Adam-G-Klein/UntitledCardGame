using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopViewController : MonoBehaviour, IShopItemViewDelegate
{
    public UIDocument uiDoc;
    public bool canDragCompanions = false;
    public Color slotHighlightColor;
    public Color slotNotHighlightColor;

    private ShopManager shopManager;
    private Dictionary<CardInShopWithPrice, ShopItemView> cardItemToViewMap;
    private Dictionary<CompanionInShopWithPrice, ShopItemView> companionItemToViewMap;

    // Specific shop VisualElement references
    private VisualElement shopGoodsArea;
    private ScrollView benchScrollView;
    private VisualElement activeContainer;

    // For dragging and dropping companions in the unit management
    private bool isDraggingCompanion = false;
    private VisualElement companionBeingDragged = null;
    private VisualElement originalParent = null;
    private Dictionary<VisualElement, Companion> visualElementToCompanionMap = new Dictionary<VisualElement, Companion>();

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
        activeContainer = uiDoc.rootVisualElement.Q("unit-active-container");
        benchScrollView = uiDoc.rootVisualElement.Q<ScrollView>("bench-scroll-view");

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
        for (int i = 0; i < companions.Count; i++) {
            VisualElement companionUI = CreateUnitManagementCompanion(companions[i]);
            activeContainer[i].Add(companionUI);
            visualElementToCompanionMap.Add(companionUI, companions[i]);
        }
    }

    public void SetupBenchCompanions(List<Companion> companions) {
        float fixedWidth = benchScrollView.contentContainer[0].resolvedStyle.width;
        // By default, the UI contains slots for 5 bench companions
        // If we have more than 5, we'll need to programatically add more
        // The +1 makes it so we always have one open slot, in case the player wants
        // to JUST move a companion from active to bench, and not swap one for another
        int slotsToAdd = companions.Count - 5 + 1;
        if (slotsToAdd > 0) {
            for (int i = 0; i < slotsToAdd; i++) {
                CreateNewBenchSlot();
            }
        }
        for (int i = 0; i < companions.Count; i++) {
            VisualElement companionUI = CreateUnitManagementCompanion(companions[i]);
            benchScrollView.contentContainer[i].Add(companionUI);
            visualElementToCompanionMap.Add(companionUI, companions[i]);
        }

        // I can't fully figure out why this is necessary. Since we're using a scroll view,
        // as more items are added, if we use a fixed percent width for the items, then as the 
        // content visual element gets bigger, the items inside update to be a certain percent
        // of the new larger size. This sets them to the size they originally have in the base
        // UI document on scene start.
        for (int i = 0; i < benchScrollView.contentContainer.childCount; i++) {
            benchScrollView.contentContainer[i].style.width = new StyleLength(fixedWidth);
        }
    }

    private void CreateNewBenchSlot() {
        VisualElement newSlot = new VisualElement();
        newSlot.AddToClassList("single-unit-container");
        benchScrollView.Add(newSlot);

        float fixedWidth = benchScrollView.contentContainer[0].resolvedStyle.width;
        newSlot.style.width = new StyleLength(fixedWidth);
    }

    public VisualElement CreateUnitManagementCompanion(Companion companion) {
        EntityView entityView = new EntityView(companion, 0, false);
        entityView.SetupEntityImage(companion.companionType.sprite);
        entityView.HideDescription();
        entityView.entityContainer.RegisterCallback<ClickEvent>(CompanionOnClick);
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

    public void CompanionOnClick(ClickEvent evt) {
        VisualElement target = evt.currentTarget as VisualElement;
        shopManager.ProcessCompanionClicked(visualElementToCompanionMap[target]);
    }

    private void CompanionOnPointerDown(PointerDownEvent evt) {
        if (!canDragCompanions && !isDraggingCompanion) return;
        VisualElement target = evt.currentTarget as VisualElement;

        VisualElement parent = target.parent;

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parent.resolvedStyle.width;
        tempContainer.style.height = parent.resolvedStyle.height;
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
        originalSpot.style.backgroundColor = slotHighlightColor;
        tempContainer.Add(companion);
        isDraggingCompanion = true;
        companionBeingDragged = companion;
    }

    private void CompanionOnPointerMove(PointerMoveEvent evt) {
        VisualElement target = evt.currentTarget as VisualElement;
        
        if (!isDraggingCompanion || companionBeingDragged != target)  return;

        target.parent.style.top = evt.position.y - target.parent.layout.height / 2;
        target.parent.style.left = evt.position.x - target.parent.layout.width / 2;

        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                child.style.backgroundColor = slotHighlightColor;
            } else {
                child.style.backgroundColor = slotNotHighlightColor;
            }
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                child.style.backgroundColor = slotHighlightColor;
            } else {
                child.style.backgroundColor = slotNotHighlightColor;
            }
        }
    }

    private void CompanionOnPointerUp(PointerUpEvent evt) {
        VisualElement target = evt.currentTarget as VisualElement;
        if (!isDraggingCompanion || target != companionBeingDragged) return;
        
        List<VisualElement> elementsOver = new List<VisualElement>();
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                elementsOver.Add(child);
            }
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                elementsOver.Add(child);
            }
        }

        if (elementsOver.Count > 0) {
            VisualElement closestContainer = elementsOver.OrderBy(x => Vector2.Distance
                (x.worldBound.position, target.worldBound.position)).First();
            DoMoveComapnion(target, closestContainer);
            closestContainer.style.backgroundColor = slotNotHighlightColor;
        } else {
            VisualElement tempContainer = target.parent;
            originalParent.Add(target);
            uiDoc.rootVisualElement.Remove(tempContainer);
        }
        isDraggingCompanion = false;
        companionBeingDragged = null;
        originalParent = null;
    }

    private void DoMoveComapnion(VisualElement companionElement, VisualElement movingToContainer) {
        // Scenario 1, dragging companion to open container
        if (movingToContainer.childCount == 0) {
            VisualElement tempContainer = companionElement.parent;
            movingToContainer.Add(companionElement);
            tempContainer.RemoveFromHierarchy();
            RefreshContainers(activeContainer.Children().ToList(), false);
            RefreshContainers(benchScrollView.contentContainer.Children().ToList(), true);
        // Scenario 2, dragging companion to slot with another companion in it already
        } else if (movingToContainer.childCount == 1) {
            originalParent.Add(movingToContainer[0]);
            VisualElement tempContainer = companionElement.parent;
            movingToContainer.Add(companionElement);
            tempContainer.RemoveFromHierarchy();
        } else {
            Debug.LogError("Companion container contains more than 1 element in heirarchy");
        }
        SetCompanionOrdering();
    }

    private void SetCompanionOrdering() {
        List<Companion> activeCompanions = new List<Companion>();
        activeContainer.Children().ToList().ForEach((ve) => {
            if (ve.childCount == 1) activeCompanions.Add(visualElementToCompanionMap[ve[0]]);
        });
        List<Companion> benchCompanions = new List<Companion>();
        benchScrollView.contentContainer.Children().ToList().ForEach((ve) => {
            if (ve.childCount == 1) benchCompanions.Add(visualElementToCompanionMap[ve[0]]);
        });
        // shopManager.SetCompanionOrdering(activeCompanions, benchCompanions);
        GetComponent<TestSetupCompanions>().SetCompanionOrdering(activeCompanions, benchCompanions);
    }

    private void RefreshContainers(List<VisualElement> unitContainers, bool isBench) {
        List<VisualElement> companions = new List<VisualElement>();
        foreach (VisualElement unitContainer in unitContainers) {
            if (unitContainer.childCount == 1) {
                companions.Add(unitContainer[0]);
            }
        }

        for (int i = 0; i < companions.Count; i++) {
            unitContainers[i].Add(companions[i]);
        }

        if (companions.Count == unitContainers.Count && isBench) {
            // This makes it so that the player can always move a companion to the bench without
            // needing to swap one companion for another
            CreateNewBenchSlot();
        }
    }
}
