using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
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
    private Dictionary<String, TooltipView> tooltipMap = new();
    private List<VisualElement> elementsWithTooltips = new();
    private GameObject tooltipPrefab;
    private TooltipController tooltipController;

    public CompendiumView(UIDocument uiDocument, CompanionPoolSO companionPool, CardPoolSO neutralCardPool, List<PackSO> packSOs, GameObject tooltipPrefab) {
        this.tooltipController = new TooltipController(tooltipPrefab);
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        this.uiDocument = uiDocument;
        this.tooltipPrefab = tooltipPrefab;
        cardsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-cards-scrollView");
        companionScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-companions-scrollView");
        cardsScrollView.Clear();
        companionScrollView.Clear();
        SetupCardView(companionPool, neutralCardPool, packSOs);
        SetupCompanionView(companionPool);
        CardButtonHandler();

        // setup buttons
        uiDocument.rootVisualElement.Q<Button>("exitButton").clicked += ExitButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("cardButton").clicked += CardButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("companionButton").clicked += CompanionButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("enemyButton").clicked += EnemyButtonHandler;

        FocusManager.Instance.RegisterFocusables(uiDocument);
        MusicController.Instance.RegisterButtonClickSFX(uiDocument);
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
        DisableCardFocusables();
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
        EnableCardFocusables();
        ResetScrollers();
    }

    public void ExitButtonHandler() {
        uiDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        FocusManager.Instance.UnregisterFocusables(uiDocument);
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        ControlsManager.Instance.UnregisterControlsReceiver(this);
    }

    private void SetupCardView(CompanionPoolSO companionPool,CardPoolSO neutralCardPool, List<PackSO> packSOs) {
        cardsSection = new VisualElement();
        cardsSection.AddToClassList("compendium-container");
        List<CompanionTypeSO> companions = companionPool.commonCompanions.Concat(companionPool.uncommonCompanions).Concat(companionPool.rareCompanions).ToList();
        companions.Sort((a, b) => a.companionName.CompareTo(b.companionName)); // we should allow for filtering by rarity or something as well...eventually  
        AddAllCompanionContainers(companions, cardsSection);
        AddAllPackContainers(packSOs, cardsSection);
        AddCards(neutralCardPool.commonCards, neutralCardPool.uncommonCards, neutralCardPool.rareCards, null, cardsSection);
        cardsScrollView.Add(cardsSection);
    }

    private void AddAllCompanionContainers(List<CompanionTypeSO> companions, VisualElement ve) {
        companions.ForEach(companion => {
            AddCards(companion.cardPool.commonCards, companion.cardPool.uncommonCards, companion.cardPool.rareCards, companion, ve);
        });
    }
    
    private void AddAllPackContainers(List<PackSO> packSOs, VisualElement ve) {
        packSOs.ForEach(packSO => {
            AddCards(packSO.packCardPoolSO.commonCards, packSO.packCardPoolSO.uncommonCards, packSO.packCardPoolSO.rareCards, null, ve, packSO);
        });
    }

    private void AddCards(List<CardType> commonCards, List<CardType> uncommonCards, List<CardType> rareCards, CompanionTypeSO companion, VisualElement ve, PackSO packSO = null)
    {
        Label companionContainerTitle = new Label();
        if (packSO != null) {
            companionContainerTitle.text = packSO.packName + " Pack Cards";
        }
        else {
            companionContainerTitle.text = companion != null ? (companion.companionName + "'s Cards") : "Neutral Cards";   
        }
        companionContainerTitle.AddToClassList("compendium-header-text");
        ve.Add(companionContainerTitle);

        VisualElement companionCardsContainer = new VisualElement();
        companionCardsContainer.AddToClassList("compendium-section-container");
        AddCardsForRarity(companionCardsContainer, commonCards, companion, Card.CardRarity.COMMON, packSO);
        AddCardsForRarity(companionCardsContainer, uncommonCards, companion, Card.CardRarity.UNCOMMON, packSO);
        AddCardsForRarity(companionCardsContainer, rareCards, companion, Card.CardRarity.RARE, packSO);
        ve.Add(companionCardsContainer);
    }

    private void AddCardsForRarity(VisualElement companionCardsContainer, List<CardType> cards, CompanionTypeSO companion, Card.CardRarity cardRarity, PackSO packSO) {
        cards.ForEach(card => {
            PackSO packToUse = companion != null ? companion.pack : packSO;
            VisualElement cardContainer = new CardView(card, companion, cardRarity, true, packToUse).cardContainer;
            cardContainer.AddToClassList("compendium-item-container");
            companionCardsContainer.Add(cardContainer);
            cardContainer.name = card.name;
            cardContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                if (card.GetTooltip().empty) {
                    return;
                }
                tooltipController.DisplayTooltip(cardContainer, card.GetTooltip(), TooltipContext.CompendiumCard);
            });
            cardContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                tooltipController.DestroyTooltip(cardContainer);
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
                VisualTreeAsset companionTemplate = EncounterConstantsSingleton.Instance.encounterConstantsSO.companionViewTemplate;
                CompanionView companionView = new CompanionView(tempCompanion, companionTemplate, 0, CompanionView.COMPENDIUM_CONTEXT, null);
                companionRow.Add(companionView.container);
                companionView.container.name = companionToDisplay.companionType.companionName + index;
                companionView.container.RegisterCallback<PointerEnterEvent>((evt) => {
                    tooltipController.DisplayTooltip(companionView.container, companionToDisplay.companionType.tooltip, TooltipContext.CompendiumCompanion);
                });
                companionView.container.RegisterCallback<PointerLeaveEvent>((evt) => {
                    tooltipController.DestroyTooltip(companionView.container);
                });
                VisualElementFocusable entityViewFocusable = companionView.container.GetUserData<VisualElementFocusable>();
                entityViewFocusable.additionalFocusAction += () => {tooltipController.DisplayTooltip(companionView.container, companionToDisplay.companionType.tooltip, TooltipContext.CompendiumCompanion);};
                entityViewFocusable.additionalUnfocusAction += () => {tooltipController.DestroyTooltip(companionView.container);};
                FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);
            };
            companionsSection.Add(companionRow);
        }
        companionScrollView.Add(companionsSection);
        DisableCompanionFocusables();
        companionScrollView.style.display = DisplayStyle.None; 
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

    private void DisableCardFocusables() {
        foreach (VisualElement ve in cardsScrollView.Query<VisualElement>(className:"focusable").ToList()) {
            FocusManager.Instance.DisableFocusableTarget(ve.AsFocusable());
        }
    }

    private void EnableCardFocusables() {
        foreach (VisualElement ve in cardsScrollView.Query<VisualElement>(className:"focusable").ToList()) {
            FocusManager.Instance.EnableFocusableTarget(ve.AsFocusable());
        }
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        switch (action) {
            case GFGInputAction.SECONDARY_UP:
                ScrollScroller(cardsScrollView, -0.1f);
                ScrollScroller(companionScrollView, -0.1f);
                FocusManager.Instance.Unfocus();
            break;

            case GFGInputAction.SECONDARY_DOWN:
                ScrollScroller(cardsScrollView, 0.1f);
                ScrollScroller(companionScrollView, 0.1f);
                FocusManager.Instance.Unfocus();
            break;
        }
    }

    private void ScrollScroller(ScrollView scrollView, float amount) {
        Scroller scroller = scrollView.verticalScroller;
        scroller.value += amount * (scroller.highValue-scroller.lowValue);
        Dictionary<string, TooltipView> dictCopy = new(tooltipMap);
        foreach (KeyValuePair<string, TooltipView> kvp in dictCopy) {
            Destroy(kvp.Value.gameObject);
            tooltipMap.Remove(kvp.Key);
        }
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