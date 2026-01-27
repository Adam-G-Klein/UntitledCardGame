using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CompendiumView : IControlsReceiver {
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
    private Button companionButton;
    private Button cardButton;

    private List<Coroutine> scrollCoroutines = new List<Coroutine>();
    
    private HashSet<CardType> unlockedCards;

    private bool cardsSetup = false;
    private bool companionsSetup = false;

    public CompendiumView(
            UIDocument uiDocument,
            CompanionPoolSO companionPool,
            CardPoolSO neutralCardPool,
            List<PackSO> packSOs,
            GameObject tooltipPrefab,
            List<CardType> unlockedCards) {
        this.tooltipController = new TooltipController(tooltipPrefab);
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        this.uiDocument = uiDocument;
        this.tooltipPrefab = tooltipPrefab;
        cardsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-cards-scrollView");
        companionScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-companions-scrollView");
        cardsScrollView.Clear();
        companionScrollView.Clear();

        this.unlockedCards = unlockedCards.ToHashSet();
        uiDocument.StartCoroutine(SetupCardView(companionPool, neutralCardPool, packSOs));
        uiDocument.StartCoroutine(SetupCompanionView(companionPool));

        // setup buttons
        companionButton = uiDocument.rootVisualElement.Q<Button>("companionButton");
        cardButton = uiDocument.rootVisualElement.Q<Button>("cardButton");

        uiDocument.rootVisualElement.Q<Button>("exitButton").clicked += ExitButtonHandler;
        cardButton.clicked += CardButtonHandler;
        companionButton.clicked += CompanionButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("enemyButton").clicked += EnemyButtonHandler;

        // FocusManager.Instance.RegisterFocusables(uiDocument);
        // MusicController.Instance.RegisterButtonClickSFX(uiDocument);
        uiDocument.StartCoroutine(RegisterControlsReceiverAtEndOfFrame());

        // CardButtonHandler();
        uiDocument.StartCoroutine(FinishInitOnceSetupComplete());
    }

    private IEnumerator RegisterControlsReceiverAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    private IEnumerator FinishInitOnceSetupComplete() {
        yield return new WaitUntil(() => cardsSetup && companionsSetup);
        FocusManager.Instance.RegisterFocusables(uiDocument);
        MusicController.Instance.RegisterButtonClickSFX(uiDocument);
        CardButtonHandler();
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

        companionButton.RemoveFromClassList("compendium-button-disabled");
        cardButton.AddToClassList("compendium-button-disabled");
        FocusManager.Instance.Unfocus();
        currentCompendiumView = CompendiumType.COMPANION;
    }

    private void CardButtonHandler()
    {
        DisableCompanionFocusables();
        cardsScrollView.style.display = DisplayStyle.Flex;
        companionScrollView.style.display = DisplayStyle.None;
        EnableCardFocusables();
        ResetScrollers();

        companionButton.AddToClassList("compendium-button-disabled");
        cardButton.RemoveFromClassList("compendium-button-disabled");
        FocusManager.Instance.Unfocus();
        currentCompendiumView = CompendiumType.CARD;
    }

    public void ExitButtonHandler() {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        FocusManager.Instance.UnregisterFocusables(uiDocument);
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        ControlsManager.Instance.UnregisterControlsReceiver(this);
        Cleanup();
        OptionsViewController.Instance.onCloseCompenidum();
    }

    // private void SetupCardView(CompanionPoolSO companionPool,CardPoolSO neutralCardPool, List<PackSO> packSOs) {
    private IEnumerator SetupCardView(CompanionPoolSO companionPool,CardPoolSO neutralCardPool, List<PackSO> packSOs) {
        cardsSection = new VisualElement();
        cardsSection.AddToClassList("compendium-container");
        cardsScrollView.Add(cardsSection);
        List<CompanionTypeSO> companions = companionPool.commonCompanions.Concat(companionPool.uncommonCompanions).Concat(companionPool.rareCompanions).ToList();
        companions.Sort((a, b) => a.companionName.CompareTo(b.companionName)); // we should allow for filtering by rarity or something as well...eventually  
        yield return AddAllCompanionContainers(companions, cardsSection);
        yield return AddAllPackContainers(packSOs, cardsSection);
        yield return AddCards(neutralCardPool, null, cardsSection);
        cardsSetup = true;
    }

    // private void AddAllCompanionContainers(List<CompanionTypeSO> companions, VisualElement ve) {
    private IEnumerator AddAllCompanionContainers(List<CompanionTypeSO> companions, VisualElement ve) {
        // companions.ForEach(companion => {
        //     yield return AddCards(companion.cardPool, companion, ve);
        // });
        foreach (CompanionTypeSO companion in companions) {
            yield return AddCards(companion.cardPool, companion, ve);
        }
    }
    
    // private void AddAllPackContainers(List<PackSO> packSOs, VisualElement ve) {
    private IEnumerator AddAllPackContainers(List<PackSO> packSOs, VisualElement ve) {
        // packSOs.ForEach(packSO => {
        //     yield return AddCards(packSO.packCardPoolSO, null, ve, packSO);
        // });
        foreach (PackSO packSO in packSOs) {
            yield return AddCards(packSO.packCardPoolSO, null, ve, packSO);
        }
    }

    // private void AddCards(CardPoolSO cardPool, CompanionTypeSO companion, VisualElement ve, PackSO packSO = null) {
    private IEnumerator AddCards(CardPoolSO cardPool, CompanionTypeSO companion, VisualElement ve, PackSO packSO = null) {
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
        AddCardsForRarity(companionCardsContainer, cardPool.commonCards, companion, Card.CardRarity.COMMON, packSO);
        AddCardsForRarity(companionCardsContainer, cardPool.uncommonCards, companion, Card.CardRarity.UNCOMMON, packSO);
        AddCardsForRarity(companionCardsContainer, cardPool.rareCards, companion, Card.CardRarity.RARE, packSO);
        AddCardsForRarity(companionCardsContainer, cardPool.unlockableCommonCards, companion, Card.CardRarity.COMMON, packSO, true);
        AddCardsForRarity(companionCardsContainer, cardPool.unlockableUncommonCards, companion, Card.CardRarity.UNCOMMON, packSO, true);
        AddCardsForRarity(companionCardsContainer, cardPool.unlockableRareCards, companion, Card.CardRarity.RARE, packSO, true);
        ve.Add(companionCardsContainer);
        yield return null;
    }

    private void AddCardsForRarity(
            VisualElement companionCardsContainer,
            List<CardType> cards,
            CompanionTypeSO companion,
            Card.CardRarity cardRarity,
            PackSO packSO,
            bool unlockable = false) {
        cards.ForEach(card => {
            PackSO packToUse = companion != null ? companion.pack : packSO;
            CardView cardView = new CardView(card, companion, cardRarity, true, packToUse);

            bool locked = false;
            if (unlockable && !unlockedCards.Contains(card)) {
                cardView.SetLocked();
                locked = true;
            }

            VisualElement cardContainer = cardView.cardContainer;
            cardContainer.AddToClassList("compendium-item-container");
            companionCardsContainer.Add(cardContainer);
            cardContainer.name = card.name;
            cardContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                if (card.GetTooltip().empty || locked) {
                    return;
                }
                tooltipController.DisplayTooltip(cardContainer, card.GetTooltip(), TooltipContext.CompendiumCard);
            });
            cardContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                tooltipController.DestroyTooltip(cardContainer);
            });
            VisualElementFocusable cardFocusable = cardContainer.AsFocusable();
            cardFocusable.canFocusOffscreen = true;
            cardFocusable.commonalityObject = cardsScrollView;
            cardFocusable.additionalFocusAction += () => {
                cardsScrollView.ScrollToMakeElementVisible(cardFocusable.GetVisualElement(), 70f); 
                MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                if (card.GetTooltip().empty || locked) {
                    return;
                }
                tooltipController.DisplayTooltip(cardContainer, card.GetTooltip(), TooltipContext.CompendiumCard);
            };
            cardFocusable.additionalUnfocusAction += () => {
                tooltipController.DestroyTooltip(cardContainer);
            };
        });
    }

    // private void SetupCompanionView(CompanionPoolSO companionPool) {
    private IEnumerator SetupCompanionView(CompanionPoolSO companionPool) {
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
            companionRow.AddToClassList("compendium-companions-section-container");

            List<Companion> companionsToDisplay = new List<Companion>
            { new Companion(companion), new Companion(companion.upgradeTo), new Companion(companion.upgradeTo.upgradeTo) };
            for (int index = 0; index < companionsToDisplay.Count; index++) {
                Companion companionToDisplay = companionsToDisplay[index];
                Companion tempCompanion = new Companion(companionToDisplay.companionType);
                VisualTreeAsset companionTemplate = EncounterConstantsSingleton.Instance.encounterConstantsSO.companionViewTemplate;
                CompanionView companionView = new CompanionView(tempCompanion, companionTemplate, 0, CompanionView.COMPENDIUM_CONTEXT, null);
                companionView.container.AddToClassList("compendium-item-container");
                companionRow.Add(companionView.container);
                companionView.container.name = companionToDisplay.companionType.companionName + index;
                companionView.container.RegisterCallback<PointerEnterEvent>((evt) => {
                    tooltipController.DisplayTooltip(companionView.container, companionToDisplay.companionType.GetTooltip(), TooltipContext.CompendiumCompanion);
                });
                companionView.container.RegisterCallback<PointerLeaveEvent>((evt) => {
                    tooltipController.DestroyTooltip(companionView.container);
                });
                VisualElementFocusable entityViewFocusable = companionView.container.GetUserData<VisualElementFocusable>();
                entityViewFocusable.canFocusOffscreen = true;
                entityViewFocusable.commonalityObject = companionScrollView;
                entityViewFocusable.additionalFocusAction += () => {
                    companionScrollView.ScrollToMakeElementVisible(entityViewFocusable.GetVisualElement(), 70f); 
                    tooltipController.DisplayTooltip(companionView.container, companionToDisplay.companionType.GetTooltip(), TooltipContext.CompendiumCompanion);
                };
                entityViewFocusable.additionalUnfocusAction += () => {
                    tooltipController.DestroyTooltip(companionView.container);
                };
                FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);
            };
            companionsSection.Add(companionRow);
            yield return null;
        }
        companionScrollView.Add(companionsSection);
        DisableCompanionFocusables();
        companionScrollView.style.display = DisplayStyle.None;
        companionsSetup = true;
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
            case GFGInputAction.SECONDARY_UP_START:
                GetCurrentScrollView().StartScrolling(0.001f, -1);
                DestroyTooltips();
                FocusManager.Instance.Unfocus();
            break;

            case GFGInputAction.SECONDARY_DOWN_START:
                GetCurrentScrollView().StartScrolling(0.001f, 1);
                DestroyTooltips();
                FocusManager.Instance.Unfocus();
            break;

            case GFGInputAction.SECONDARY_DOWN_END:
            case GFGInputAction.SECONDARY_UP_END:
                GetCurrentScrollView().StopScrolling();
            break;
        }
    }

    private ScrollView GetCurrentScrollView() {
        if (currentCompendiumView == CompendiumType.COMPANION) {
            return companionScrollView;
        } else {
            return cardsScrollView;
        }
    }

    private void DestroyTooltips() {
        Dictionary<string, TooltipView> dictCopy = new(tooltipMap);
        foreach (KeyValuePair<string, TooltipView> kvp in dictCopy) {
            GameObject.Destroy(kvp.Value.gameObject);
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

    public void Cleanup() {
        // likely not necessary, but was getting some lag after working on compendium view for awhile so I am worried about a memory leak
        // Unsubscribe from events
        uiDocument.rootVisualElement.Q<Button>("exitButton").clicked -= ExitButtonHandler;
        cardButton.clicked -= CardButtonHandler;
        companionButton.clicked -= CompanionButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("enemyButton").clicked -= EnemyButtonHandler;

        // Additional cleanup logic if necessary
        cardsScrollView?.Clear();
        companionScrollView?.Clear();
    }
}