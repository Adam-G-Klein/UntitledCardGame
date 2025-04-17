using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.ComponentModel;
using System.Linq;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CompendiumView : MonoBehaviour, IControlsReceiver {
    public VisualElement compendiumContainer;
    private UIDocument uiDocument;
    private CompendiumType currentCompendiumView = CompendiumType.CARD;
    private ScrollView cardsScrollView;
    private ScrollView companionScrollView;
    private enum CompendiumType {
        COMPANION,
        CARD, 
        ENEMY
    }
    private VisualElement companionsSection;
    private VisualElement cardsSection;
    private Dictionary<String, GameObject> tooltipMap = new();
    private GameObject tooltipPrefab;

    public CompendiumView(UIDocument uiDocument, CompanionPoolSO companionPool, CardPoolSO neutralCardPool, GameObject tooltipPrefab) {
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        this.uiDocument = uiDocument;
        this.tooltipPrefab = tooltipPrefab;
        cardsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-cards-scrollView");
        companionScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-companions-scrollView");
        cardsScrollView.Clear();
        companionScrollView.Clear();
        SetupCardView(companionPool, neutralCardPool);
        SetupCompanionView(companionPool);
        CardButtonHandler();

        // setup buttons
        uiDocument.rootVisualElement.Q<Button>("exitButton").clicked += ExitButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("cardButton").clicked += CardButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("companionButton").clicked += CompanionButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("enemyButton").clicked += EnemyButtonHandler;

        FocusManager.Instance.RegisterFocusables(uiDocument);
        uiDocument.StartCoroutine(RegisterControlsReceiverAtEndOfFrame());
    }

    private IEnumerator RegisterControlsReceiverAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    private void EnemyButtonHandler()
    {
        throw new NotImplementedException();
    }

    private void CompanionButtonHandler()
    {
        cardsScrollView.style.display = DisplayStyle.None;
        companionScrollView.style.display = DisplayStyle.Flex;
        EnableCompanionFocusables();
        ResetScrollers();
    }

    private void CardButtonHandler()
    {
        DisableCompanionFocusables();
        cardsScrollView.style.display = DisplayStyle.Flex;
        companionScrollView.style.display = DisplayStyle.None;
        ResetScrollers();
    }

    public void ExitButtonHandler() {
        uiDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        FocusManager.Instance.UnregisterFocusables(uiDocument);
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        ControlsManager.Instance.UnregisterControlsReceiver(this);
    }

    private void SetupCardView(CompanionPoolSO companionPool,CardPoolSO neutralCardPool) {
        cardsSection = new VisualElement();
        cardsSection.AddToClassList("compendium-container");
        List<CompanionTypeSO> companions = companionPool.commonCompanions.Concat(companionPool.uncommonCompanions).Concat(companionPool.rareCompanions).ToList();
        companions.Sort((a, b) => a.companionName.CompareTo(b.companionName)); // we should allow for filtering by rarity or something as well...eventually  
        AddAllCompanionContainers(companions, cardsSection);
        AddCards(neutralCardPool.commonCards, neutralCardPool.uncommonCards, neutralCardPool.rareCards, null, cardsSection);
        cardsScrollView.Add(cardsSection);
    }

    private void AddAllCompanionContainers(List<CompanionTypeSO> companions, VisualElement ve) {
        companions.ForEach(companion => {
            List<CardType> cards = new List<CardType>();
            cards = companion.cardPool.commonCards.Concat(companion.cardPool.uncommonCards).Concat(companion.cardPool.rareCards).ToList();
            AddCards(companion.cardPool.commonCards, companion.cardPool.uncommonCards, companion.cardPool.rareCards, companion, ve);
        });
    }

    private void AddCards(List<CardType> commonCards, List<CardType> uncommonCards, List<CardType> rareCards, CompanionTypeSO companion, VisualElement ve) {
        Label companionContainerTitle = new Label();
        companionContainerTitle.text = companion != null ? (companion.companionName + "'s Cards") : "Neutral Cards";
        companionContainerTitle.AddToClassList("compendium-header-text");
        ve.Add(companionContainerTitle);
        
        VisualElement companionCardsContainer = new VisualElement();
        companionCardsContainer.AddToClassList("compendium-section-container");
        AddCardsForRarity(companionCardsContainer, commonCards, companion, Card.CardRarity.COMMON);
        AddCardsForRarity(companionCardsContainer, uncommonCards, companion, Card.CardRarity.UNCOMMON);
        AddCardsForRarity(companionCardsContainer, rareCards, companion, Card.CardRarity.RARE);
        ve.Add(companionCardsContainer);
    }

    private void AddCardsForRarity(VisualElement companionCardsContainer, List<CardType> cards, CompanionTypeSO companion, Card.CardRarity cardRarity) {
        cards.ForEach(card => {
            VisualElement cardContainer = new CardView(card, companion, cardRarity, true).cardContainer;
            cardContainer.AddToClassList("compendium-item-container");
            companionCardsContainer.Add(cardContainer);
            cardContainer.name = card.name;
            cardContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                if (card.GetTooltip().empty) return;
                Debug.LogError("hovering:" + card.name);
                DisplayTooltip(cardContainer, card.GetTooltip());
            });
            cardContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                Debug.LogError("unhovering:" + card.name);
                DestroyTooltip(cardContainer);
            });
        });
    }

    private void SetupCompanionView(CompanionPoolSO companionPool) {
        companionsSection = new VisualElement();
        companionsSection.AddToClassList("compendium-container");
        List<CompanionTypeSO> companions = companionPool.commonCompanions.Concat(companionPool.uncommonCompanions).Concat(companionPool.rareCompanions).ToList();
        for (int i = 0; i < companions.Count; i++) {
            CompanionTypeSO companion = companions[i];
    

            Label sectionTitle = new Label();
            sectionTitle.text = companion.companionName + "'s Upgrades!";
            sectionTitle.AddToClassList("compendium-header-text");
            companionsSection.Add(sectionTitle);

            VisualElement companionRow = new VisualElement();
            companionRow.AddToClassList("compendium-section-container");

            List<Companion> companionsToDisplay = new List<Companion>
            { new Companion(companion), new Companion(companion.upgradeTo), new Companion(companion.upgradeTo.upgradeTo) };
            for (int index = 0; index < companionsToDisplay.Count; index++) {
                Companion companionToDisplay = companionsToDisplay[index];
                Companion tempCompanion = new Companion(companionToDisplay.companionType);
                EntityView entityView = new EntityView(tempCompanion, 0, false);
                entityView.entityContainer.AddToClassList("compendium-item-container");
                VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
                portraitContainer.style.backgroundImage = new StyleBackground(companion.sprite);
                companionRow.Add(entityView.entityContainer);
                entityView.entityContainer.name = companionToDisplay.companionType.companionName + index;
                entityView.entityContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                    Debug.LogError("hovering");
                    DisplayTooltip(entityView.entityContainer, companionToDisplay.companionType.tooltip);
                });
                entityView.entityContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                    DestroyTooltip(entityView.entityContainer);
                });
                VisualElementFocusable entityViewFocusable = entityView.focusableElement.AsFocusable();
                entityViewFocusable.additionalFocusAction = () => {DisplayTooltip(entityView.entityContainer, companionToDisplay.companionType.tooltip);};
                entityViewFocusable.additionalUnfocusAction = () => {DestroyTooltip(entityView.entityContainer);};
                FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);
            };
            companionsSection.Add(companionRow);
        }
        companionScrollView.Add(companionsSection);
        DisableCompanionFocusables();
        companionScrollView.style.display = DisplayStyle.None; 
    }

    public void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel) {
        if (tooltipMap.ContainsKey(element.name)) {
            return;
        }
        Vector3 tooltipPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);

        tooltipPosition.x += element.resolvedStyle.width / 120; // this feels super brittle 
        tooltipPosition.y += element.resolvedStyle.width / 150;
        
        GameObject uiDocToolTipPrefab = Instantiate(tooltipPrefab, new Vector3(tooltipPosition.x, tooltipPosition.y, -1), new Quaternion());
        TooltipView tooltipView = uiDocToolTipPrefab.GetComponent<TooltipView>();
        tooltipView.tooltip = tooltipViewModel;

        tooltipMap[element.name] = uiDocToolTipPrefab;
    }

    public void DestroyTooltip(VisualElement element) {
        if(tooltipMap.ContainsKey(element.name)) {
            Destroy(tooltipMap[element.name]);
            tooltipMap.Remove(element.name);
        }
    }

    private void DisableCompanionFocusables() {
        foreach (VisualElement ve in companionScrollView.Query<VisualElement>(className:"focusable").ToList()) {
            FocusManager.Instance.DisableFocusableTarget(ve.AsFocusable());
        }
    }

    private void EnableCompanionFocusables() {
        foreach (VisualElement ve in companionScrollView.Query<VisualElement>(className:"focusable").ToList()) {
            FocusManager.Instance.EnableFocusableTarget(ve.AsFocusable());
        }
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        switch (action) {
            case GFGInputAction.SECONDARY_UP:
                ScrollScroller(cardsScrollView, -0.1f);
                ScrollScroller(companionScrollView, -0.1f);
            break;

            case GFGInputAction.SECONDARY_DOWN:
                ScrollScroller(cardsScrollView, 0.1f);
                ScrollScroller(companionScrollView, 0.1f);
            break;
        }
    }

    private void ScrollScroller(ScrollView scrollView, float amount) {
        Scroller scroller = scrollView.verticalScroller;
        scroller.value += amount * (scroller.highValue-scroller.lowValue);
    }

    private void ResetScrollers() {
        cardsScrollView.verticalScroller.value = cardsScrollView.verticalScroller.lowValue;
        companionScrollView.verticalScroller.value = companionScrollView.verticalScroller.lowValue;
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }
}