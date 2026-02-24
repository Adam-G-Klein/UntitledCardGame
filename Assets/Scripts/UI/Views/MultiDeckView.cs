using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public class MultiDeckView
{
    private int currentTabIndex = 0;
    private VisualElement tabsContainer;
    public IconButton leftButton { get; private set; }
    public IconButton rightButton { get; private set; }
    public List<Button> tabButtons;
    public VisualElement tabLeftIcon;
    public VisualElement tabRightIcon;
    public IconButton exitButton { get; private set; }
    [SerializeField]
    private UIDocument uiDocument;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private float animationDuration = 0.25f;
    [SerializeField]
    private LeanTweenType animationEaseType = LeanTweenType.easeInOutSine;
    private Vector2 CARD_SIZE = new();
    private VisualElement visibilityRoot;
    private IMultiDeckViewDelegate multiDeckViewDelegate;
    private bool inTransition = false;
    private Label nameLabel;
    private VisualElement tabButtonContainer;
    private string TAB_DESCRIPTOR = "tab-descriptor-{0}";
    private bool isEnabled = true;
    private List<DeckTab> deckTabs;
    private List<DeckTabView> deckTabViews;

    private TooltipController tooltipController;

    private Dictionary<int, int> CARDS_WIDE_PER_COMP_NUM = new Dictionary<int, int>() {
        { 1, 8 },
        { 2, 6 },
        { 3, 5 },
        { 4, 4 },
        { 5, 3 }
    };

    public MultiDeckView(IMultiDeckViewDelegate multiDeckViewDelegate, UIDocument uIDocument, CanvasGroup canvasGroup, GameObject tooltipPrefab)
    {
        this.tooltipController = new TooltipController(tooltipPrefab);
        this.uiDocument = uIDocument;
        this.canvasGroup = canvasGroup;
        this.multiDeckViewDelegate = multiDeckViewDelegate;
        this.tabButtons = new List<Button>();
        this.deckTabViews = new List<DeckTabView>();

        Tuple<int, int> cardSizeTuple = CardView.GetWidthAndHeight();
        CARD_SIZE = new Vector2(cardSizeTuple.Item1, cardSizeTuple.Item2);
        tabsContainer = uiDocument.rootVisualElement.Q("SectionsContainer");
        visibilityRoot = uiDocument.rootVisualElement.Q("rootElement");
        nameLabel = uiDocument.rootVisualElement.Q<Label>("multi-deck-view-name-label");
        tabButtonContainer = uiDocument.rootVisualElement.Q<VisualElement>("multi-deck-view-tab-button-container");
        tabLeftIcon = uiDocument.rootVisualElement.Q<VisualElement>("multi-deck-view-tab-left-icon");
        tabRightIcon = uiDocument.rootVisualElement.Q<VisualElement>("multi-deck-view-tab-right-icon");
        ToggleVisibility(false);
        SetupButtons();
    }

    public void PopulateDeckView(List<DeckTab> deckTabs, int startingTab = 0, int startingIndex = 0)
    {
        this.deckTabs = deckTabs;
        this.deckTabViews = new List<DeckTabView>();
        currentTabIndex = 0;
        tabButtonContainer.Clear();
        for (int i = 0; i < deckTabs.Count; i++)
        {
            DeckTab tab = deckTabs[i];
            DeckTabView deckTabView = new DeckTabView(tabsContainer.Children().ToList()[i], tab);
            deckTabViews.Add(deckTabView);
            SetupCards(deckTabView, i == startingTab, i == startingTab ? startingIndex : 0);
            Button newButton = new Button();
            newButton.AddToClassList("multi-deck-view-tab-button");
            newButton.AddToClassList("multi-deck-view-focus-item");
            newButton.RegisterOnSelected(TabButtonClicked);
            newButton.text = tab.title;
            newButton.SetUserData<TabButton>(new TabButton(i));
            tabButtonContainer.Add(newButton);
            if (i == startingTab)
            {
                MoveInTab(deckTabView.container, false, true);
                newButton.AddToClassList("multi-deck-view-tab-button-selected");
            }
            else
            {
                MoveOutTab(deckTabView.container, false, true);
            }
        }
        currentTabIndex = startingTab;
        nameLabel.text = deckTabViews[startingTab].sectionViews[startingIndex].deckTabSection.companion.GetName();

        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController) {
            // Wait two frames to update the tab button icons
            tabButtonContainer.schedule.Execute(() => { 
                tabButtonContainer.schedule.Execute(() => { 
                    SetTabButtonIconsPositionAndVisible(true); 
                }).StartingIn(0);
            }).StartingIn(0);
        }

        // hover first card
        List<VisualElement> focusedDeck = deckTabViews[currentTabIndex].focusedSectionView.scrollView.Children().ToList();
        if (focusedDeck.Count == 0) return;
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;
        VisualElement firstCard = focusedDeck[0];
        // Trying to solve some weirdness with timing and tooltips
        FocusManager.Instance.Unfocus();
    }

    private void SetupButtons()
    {
        // leftButton and rightButton move which companion you're looking at
        // setup leftButton
        leftButton = uiDocument.rootVisualElement.Q<IconButton>("leftButton");
        leftButton.RegisterOnSelected(LeftButtonClicked);
        leftButton.SetIconRight();
        leftButton.SetIconHeight(1.25f);

        leftButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_SECTION_LEFT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_SECTION_LEFT));
        ControlsManager.Instance.RegisterIconChanger(leftButton);

        // setup rightButton
        rightButton = uiDocument.rootVisualElement.Q<IconButton>("rightButton");
        rightButton.RegisterOnSelected(RightButtonClicked);
        rightButton.SetIconHeight(1.25f);

        rightButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_SECTION_RIGHT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_SECTION_RIGHT));
        ControlsManager.Instance.RegisterIconChanger(rightButton);

        // TODO Setup IconChanger visual elements for tab buttons
        IconVisualElement leftTabIconVE = new IconVisualElement(tabLeftIcon);
        leftTabIconVE.SetIcon(GFGInputAction.MULTI_DECK_VIEW_TAB_LEFT, 
                ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_TAB_LEFT));
        ControlsManager.Instance.RegisterIconChanger(leftTabIconVE);

        IconVisualElement rightTabIconVE = new IconVisualElement(tabRightIcon);
        rightTabIconVE.SetIcon(GFGInputAction.MULTI_DECK_VIEW_TAB_RIGHT, 
                ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_TAB_RIGHT));
        ControlsManager.Instance.RegisterIconChanger(rightTabIconVE);

        exitButton = uiDocument.rootVisualElement.Q<IconButton>("exitButton");
        exitButton.RegisterOnSelected(ExitButtonClicked);
        exitButton.SetIcon(
            GFGInputAction.BACK,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.BACK));
        ControlsManager.Instance.RegisterIconChanger(exitButton);
    }

    public void RegisterFocusables()
    {
        FocusManager.Instance.RegisterFocusableTarget(leftButton.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(rightButton.AsFocusable());
        // FocusManager.Instance.RegisterFocusableTarget(tabLeftButton.AsFocusable());
        // FocusManager.Instance.RegisterFocusableTarget(tabRightButton.AsFocusable());
        // TODO setup tab buttons for focus, loop through tabButtonContainer children
        FocusManager.Instance.RegisterFocusableTarget(exitButton.AsFocusable());
    }

    public void UnregisterFocusables()
    {
        FocusManager.Instance.UnregisterFocusableTarget(leftButton.AsFocusable());
        FocusManager.Instance.UnregisterFocusableTarget(rightButton.AsFocusable());
        // FocusManager.Instance.UnregisterFocusableTarget(tabLeftButton.AsFocusable());
        // FocusManager.Instance.UnregisterFocusableTarget(tabRightButton.AsFocusable());
        // TODO setup tab buttons for unregister, loop through tabButtonContainer children
        FocusManager.Instance.UnregisterFocusableTarget(exitButton.AsFocusable());
    }

    private void UnFocusDeckSection(DeckSectionView deckView, bool instant = false)
    {
        LeanTween.value(1f, 0f, instant ? 0f : animationDuration)
        .setOnUpdate((float val) =>
        {
            deckView.container.style.flexGrow = val;
        })
        .setEase(animationEaseType);

        ToggleDeckFocusability(deckView.scrollView.contentContainer.Children().ToList(), false);
        VisualElement companionView = deckView.companionView.container;
        if (companionView != null) FocusManager.Instance.DisableFocusableTarget(companionView.AsFocusable());
        SetPositionForUnSelectedSection(deckView.container, instant);
    }

    private void FocusDeckSection(DeckSectionView deckView, bool instant = false, bool enableFocusables = true)
    {
        inTransition = true;
        ScrollView scrollView = deckView.scrollView;
        VisualElement companionView = deckView.companionView.container;
        LeanTween.value(0f, 1f, instant ? 0f : animationDuration)
            .setOnUpdate((float val) =>
            {
                deckView.container.style.flexGrow = val;
            })
            .setEase(animationEaseType)
            .setOnComplete(() => {
                OnFocusComplete(scrollView, companionView, enableFocusables);
            });
        SetPositionForSelectedSection(scrollView, instant);
    }
    public void OnFocusComplete(ScrollView scrollView, VisualElement companionView, bool enableFocusables = true)
    {
        // ideally do after a slight delay
        inTransition = false;
        if (scrollView.contentContainer.Children().ToList().Count == 0) return;
        if (enableFocusables) {
            ToggleDeckFocusability(scrollView.contentContainer.Children().ToList(), true);
            if (companionView != null) FocusManager.Instance.EnableFocusableTarget(companionView.AsFocusable());
            if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;
            VisualElement firstCard = scrollView.contentContainer.Children().ToList()[0];
            FocusManager.Instance.SetFocusNextFrame(firstCard.AsFocusable());
        }
    }

    private void ToggleDeckFocusability(List<VisualElement> elements, bool enable)
    {
        foreach (VisualElement element in elements)
        {
            if (element.HasUserData<VisualElementFocusable>())
            {
                if (enable) FocusManager.Instance.EnableFocusableTarget(element.AsFocusable());
                else FocusManager.Instance.DisableFocusableTarget(element.AsFocusable());
            }
        }
    }

    public void LeftButtonClicked(ClickEvent evt)
    {
        tooltipController.DestroyAllTooltips();
        DeckTabView deckTabView = deckTabViews[currentTabIndex];
        DeckSectionView deckSectionView = deckTabView.focusedSectionView;
        int indexOfSection = deckTabView.sectionViews.IndexOf(deckSectionView);

        if (inTransition || indexOfSection == 0) return; // remove this line to enable wrap around
        UnFocusDeckSection(deckSectionView, false);
        
        FocusDeckSection(deckTabView.sectionViews[indexOfSection - 1], false);
        deckTabView.focusedSectionView = deckTabView.sectionViews[indexOfSection - 1];
        nameLabel.text = deckTabView.focusedSectionView.deckTabSection.companion.GetName();
    }

    public void RightButtonClicked(ClickEvent evt)
    {
        tooltipController.DestroyAllTooltips();
        DeckTabView deckTabView = deckTabViews[currentTabIndex];
        DeckSectionView deckSectionView = deckTabView.focusedSectionView;

        int indexOfSection = deckTabView.sectionViews.IndexOf(deckSectionView);

        if (inTransition || indexOfSection == deckTabView.sectionViews.Count - 1) return; // remove this line to enable wrap around

        UnFocusDeckSection(deckSectionView, false);

        FocusDeckSection(deckTabView.sectionViews[indexOfSection + 1], false);
        deckTabView.focusedSectionView = deckTabView.sectionViews[indexOfSection + 1];
        nameLabel.text = deckTabView.focusedSectionView.deckTabSection.companion.GetName();
    }

    public void TabLeftButtonClicked(ClickEvent evt)
    {
        tooltipController.DestroyAllTooltips();
        if (inTransition || currentTabIndex == 0) return; // remove this line to enable wrap around

        DeckTabView currDeckTabView = deckTabViews[currentTabIndex];
        MoveOutTab(currDeckTabView.container, true);
        ToggleDeckFocusability(currDeckTabView.focusedSectionView.scrollView.Children().ToList(), false);
        int newIndex = currentTabIndex - 1;
        DeckTabView newDeckTabView = deckTabViews[newIndex];
        MoveInTab(newDeckTabView.container, true);
        currentTabIndex = newIndex;
        ToggleDeckFocusability(newDeckTabView.focusedSectionView.scrollView.Children().ToList(), true);
        SetTabButtonStyle(tabButtonContainer.Children().ToList()[currentTabIndex]);
        nameLabel.text = nameLabel.text = newDeckTabView.focusedSectionView.deckTabSection.companion.GetName();
    }

    public void TabRightButtonClicked(ClickEvent evt)
    {
        tooltipController.DestroyAllTooltips();
        if (inTransition || currentTabIndex == deckTabViews.Count - 1) return; // remove this line to enable wrap around

        DeckTabView currDeckTabView = deckTabViews[currentTabIndex];
        MoveOutTab(currDeckTabView.container, false);
        ToggleDeckFocusability(currDeckTabView.focusedSectionView.scrollView.Children().ToList(), false);
        int newIndex = currentTabIndex + 1;
        DeckTabView newDeckTabView = deckTabViews[newIndex];
        MoveInTab(newDeckTabView.container, false);
        currentTabIndex = newIndex;
        ToggleDeckFocusability(newDeckTabView.focusedSectionView.scrollView.Children().ToList(), true);
        SetTabButtonStyle(tabButtonContainer.Children().ToList()[currentTabIndex]);
        nameLabel.text = nameLabel.text = newDeckTabView.focusedSectionView.deckTabSection.companion.GetName();
    }

    public void ExitButtonClicked(ClickEvent evt)
    {
        MoveOutTab(deckTabViews[currentTabIndex].container, true, true);
        ToggleVisibility(false);
        multiDeckViewDelegate.HideDeckView();
        // Cleanup
        CleanUp();
    }

    public void TabButtonClicked(ClickEvent evt) {
        tooltipController.DestroyAllTooltips();
        VisualElement ve = evt.target as VisualElement;
        int tabIndex = ve.GetUserData<TabButton>().index;

        if (currentTabIndex == tabIndex) return;

        bool sendRight = true;
        if (currentTabIndex < tabIndex) sendRight = false;

        DeckTabView currDeckTabView = deckTabViews[currentTabIndex];
        MoveOutTab(currDeckTabView.container, sendRight);
        ToggleDeckFocusability(currDeckTabView.focusedSectionView.scrollView.Children().ToList(), false);

        int newIndex = tabIndex;
        DeckTabView newDeckTabView = deckTabViews[newIndex];
        MoveInTab(newDeckTabView.container, sendRight);
        currentTabIndex = newIndex;
        ToggleDeckFocusability(newDeckTabView.focusedSectionView.scrollView.Children().ToList(), true);
        nameLabel.text = nameLabel.text = newDeckTabView.focusedSectionView.deckTabSection.companion.GetName();
        SetTabButtonStyle(ve);
    }

    private void SetTabButtonStyle(VisualElement ve) {
        foreach (VisualElement element in tabButtonContainer.Children()) {
            if (element.Equals(ve)) {
                element.AddToClassList("multi-deck-view-tab-button-selected");
            } else {
                element.RemoveFromClassList("multi-deck-view-tab-button-selected");
            }
        }
    }

    public void StartScrolling(int direction) {
        GetCurrentScrollView().StartScrolling(0.015f, direction);
    }

    public void StopScrolling() {
        GetCurrentScrollView().StopScrolling();
    }

    private ScrollView GetCurrentScrollView() {
        return deckTabViews[currentTabIndex].focusedSectionView.scrollView;
    }

    private void MoveOutTab(VisualElement visualElementToMoveOut, bool sendRight, bool instant = false)
    {
        float newPos = (sendRight ? 1 : -1) * visualElementToMoveOut.resolvedStyle.width;
        if (instant)
        {
            visualElementToMoveOut.style.left = newPos;
        }
        else
        {
            inTransition = true;
            LeanTween.value(0f, newPos, animationDuration)
                .setOnUpdate((float val) =>
                {
                    visualElementToMoveOut.style.left = val;
                })
            .setEase(animationEaseType);
        }
    }
    private void MoveInTab(VisualElement visualElementToMoveIn, bool sendRight, bool instant = false)
    {
        if (instant)
        {
            visualElementToMoveIn.style.left = 0;
            return;
        }
        float startPos = (sendRight ? -1 : 1) * visualElementToMoveIn.resolvedStyle.width;
        visualElementToMoveIn.style.left = startPos;

        // Animate with LeanTween
        LeanTween.value(startPos, 0f, animationDuration)
            .setOnUpdate((float val) =>
            {
                visualElementToMoveIn.style.left = val;
            })
        .setEase(animationEaseType)
        .setOnComplete(() =>
        {
            inTransition = false;
        });
    }

    private void SetupCards(DeckTabView deckTabView, bool isStartingTab, int startingIndex = 0)
    {
        for (int i = 0; i < deckTabView.sectionViews.Count; i++)
        {
            DeckSectionView deckSectionView = deckTabView.sectionViews[i];
            if (i >= deckTabView.deckTab.sections.Count)
            {
                return;
            }

            deckSectionView.container.style.display = DisplayStyle.Flex;
            Companion companion = deckTabView.sectionViews[i].deckTabSection.companion;
            VisualTreeAsset companionTemplate = EncounterConstantsSingleton.Instance.encounterConstantsSO.companionManagementViewTemplate;
            CompanionManagementView companionView = new CompanionManagementView(companion, companionTemplate, null);
            VisualElementFocusable companionFocusable = companionView.container.AsFocusable();
            companionView.container.RegisterCallback<PointerEnterEvent>((evt) => {
                tooltipController.DisplayTooltip(companionView.container, companion.companionType.GetTooltip(), TooltipContext.MultiDeckView);
            });
            companionView.container.RegisterCallback<PointerLeaveEvent>((evt) => {
                tooltipController.DestroyTooltip(companionView.container);
            });

            // Don't put the SFX in the RegisterCallbacks for companions above, it's already done in
            // CompanionManagementView
            companionFocusable.additionalFocusAction += () => {
                MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                tooltipController.DisplayTooltip(companionView.container, companion.companionType.GetTooltip(), TooltipContext.MultiDeckView);
            };
            companionFocusable.additionalUnfocusAction += () => {
                MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                tooltipController.DestroyTooltip(companionView.container);
            };
            FocusManager.Instance.RegisterFocusableTarget(companionFocusable);
            companionView.container.AddToClassList("companionView");
            companionView.container.AddToClassList("multi-deck-view-focus-item");
            companionView.UpdateWidthAndHeight(.15f);
            deckSectionView.header.Add(companionView.container);
            deckSectionView.companionView = companionView;
            if (i >= deckTabView.sectionViews.Count - 1) {
                // The divider is on the right of each container. The last one shouldn't have
                // the divider because it'd just overlap with the overall frame
                deckSectionView.divider.style.visibility = Visibility.Hidden;
            } else {
                deckSectionView.divider.style.visibility = Visibility.Visible;
            }

            // Note, this is only valid for during combat. In the shop, we will have no valid combat instance.
            CompanionInstance ci = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(i);

            List<Card> cards = deckTabView.sectionViews[i].deckTabSection.cards;
            for (int j = 0; j < cards.Count; j++)
            {
                Card card = cards[j];
                CardView cardView = new CardView(card, companion.companionType, true);
                cardView.cardContainer.AddToClassList("cardView");
                cardView.cardContainer.AddToClassList("multi-deck-view-focus-item");
                cardView.cardContainer.MakeFocusable();
                deckSectionView.scrollView.Add(cardView.cardContainer);
                cardView.cardFocusable.canFocusOffscreen = true;
                cardView.cardFocusable.commonalityObject = deckSectionView.scrollView;
                FocusManager.Instance.RegisterFocusableTarget(cardView.cardFocusable);

                cardView.cardContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                    MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                    if (card.cardType.GetTooltip().empty) {
                        return;
                    }
                    tooltipController.DisplayTooltip(cardView.cardContainer, card.cardType.GetTooltip(), TooltipContext.MultiDeckView);
                });
                cardView.cardContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                    tooltipController.DestroyTooltip(cardView.cardContainer);
                });
                VisualElementFocusable cardFocusable = cardView.cardContainer.AsFocusable();
                cardFocusable.additionalFocusAction += () => {
                    deckSectionView.scrollView.ScrollToMakeElementVisible(cardFocusable.GetVisualElement(), 20f);
                    MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
                    if (card.cardType.GetTooltip().empty) {
                        return;
                    }
                    tooltipController.DisplayTooltip(cardView.cardContainer, card.cardType.GetTooltip(), TooltipContext.MultiDeckView);
                };
                cardFocusable.additionalUnfocusAction += () => {
                    tooltipController.DestroyTooltip(cardView.cardContainer);
                };

                // Hack to display the modified card values when pulling up the deck values.
                if (ci != null)
                {
                    Dictionary<string, int> defaultValuesDict = card.GetDefaultValuesMap(ci.combatInstance);
                    if (card.cardType.HasIconDescription())
                    {
                        List<DescriptionToken> iconTokens = card.cardType.GetIconDescriptionTokensWithStylizedValues(defaultValuesDict);
                        cardView.UpdateCardIconDescription(iconTokens);
                    }
                    else
                    {
                        string modText = card.cardType.GetDescriptionWithUpdatedValues(defaultValuesDict);
                        cardView.UpdateCardText(modText);
                    }

                    cardView.UpdateManaCost();
                }
            }

            if ((isStartingTab && i == startingIndex) || (!isStartingTab && i == 0))
            {
                FocusDeckSection(deckSectionView, true, isStartingTab && i == startingIndex);
                deckTabView.focusedSectionView = deckSectionView;
            }
            else
            {
                UnFocusDeckSection(deckSectionView, true);
            }
        }
    }

    private void SetPositionForSelectedSection(ScrollView scrollView, bool instant = false)
    {
        Vector2 basePosition = new Vector2(CARD_SIZE.x / 2 + 20, 20);
        float X_STEP = CARD_SIZE.x + 20;
        float Y_STEP = CARD_SIZE.y + 20;
        List<VisualElement> scrollViewContents = scrollView.contentContainer.Children().ToList();
        for (int i = 0; i < scrollViewContents.Count; i++)
        {
            VisualElement element = scrollViewContents[i];
            float initialTop = element.resolvedStyle.top;
            float initialLeft = element.resolvedStyle.left;
            float finalLeft = basePosition.x + X_STEP * (i % CARDS_WIDE_PER_COMP_NUM[deckTabViews[currentTabIndex].sectionViews.Count]);
            float finalTop = basePosition.y + Y_STEP * Mathf.Floor(i / CARDS_WIDE_PER_COMP_NUM[deckTabViews[currentTabIndex].sectionViews.Count]);
            LeanTween.value(0f, 1f, instant ? 0f : animationDuration)
            .setOnUpdate((float val) =>
            {
                element.style.top = Mathf.Lerp(initialTop, finalTop, val);
                element.style.left = Mathf.Lerp(initialLeft, finalLeft, val);
            })
            .setEase(animationEaseType)
            .setOnComplete(() =>
            {
                element.style.top = finalTop;
                element.style.left = finalLeft;
            });
        }
    }

    private void SetPositionForUnSelectedSection(VisualElement sectionContainer, bool instant = false)
    {
        ScrollView scrollView = sectionContainer.Q<ScrollView>("ScrollView");
        float maxYPos = scrollView.resolvedStyle.height - CARD_SIZE.y - 10;
        List<VisualElement> scrollViewContents = scrollView.contentContainer.Children().ToList();
        for (int j = 0; j < scrollViewContents.Count; j++)
        {
            VisualElement element = scrollViewContents[j];
            float initialTop = element.resolvedStyle.top;
            float initialLeft = element.resolvedStyle.left;
            float finalTop = j * maxYPos / (scrollViewContents.Count - 1);
            float finalLeft = sectionContainer.resolvedStyle.minWidth.value / 2;

            LeanTween.value(0f, 1f, instant ? 0f : animationDuration)
            .setOnUpdate((float val) =>
            {
                element.style.top = Mathf.Lerp(initialTop, finalTop, val);
                element.style.left = Mathf.Lerp(initialLeft, finalLeft, val);
            })
            .setEase(animationEaseType)
            .setOnComplete(() =>
            {
                element.style.top = finalTop;
                element.style.left = finalLeft;
            });
        }
    }

    public void ToggleVisibility(bool visible = false)
    {
        if (!isEnabled) return;
        if (visible)
        {
            canvasGroup.blocksRaycasts = true;
            UIDocumentUtils.SetAllPickingMode(uiDocument.rootVisualElement, PickingMode.Position);
            visibilityRoot.style.visibility = Visibility.Visible;
        }
        else
        {
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(uiDocument.rootVisualElement, PickingMode.Ignore);
            visibilityRoot.style.visibility = Visibility.Hidden;
            // These get set specifically because they're style is overridden so they
            // don't inherit from parent
            tabLeftIcon.style.visibility = Visibility.Hidden;
            tabRightIcon.style.visibility = Visibility.Hidden;
        }
    }

    public void CleanUp()
    {
        UnregisterFocusables();
        foreach (DeckTabView deckTabView in deckTabViews)
        {
            foreach (DeckSectionView section in deckTabView.sectionViews)
            {
                section.header.Clear();
                section.scrollView.Clear();
                UnFocusDeckSection(section, true);
            }
        }
        tooltipController.DestroyAllTooltips();
    }

    public void TurnOffInteractions() {
        ToggleVisibility(false);
        CleanUp();
        isEnabled = false;
    }

    public void SetTabButtonIconsPositionAndVisible(bool visible) {
        Visibility visibility;

        if (visible && deckTabViews.Count != 1) {
            visibility = Visibility.Visible;
            List<VisualElement> buttons = tabButtonContainer.Children().ToList();
            VisualElement first = buttons.First();
            VisualElement last = buttons.Last();
            tabLeftIcon.style.left = first.worldBound.xMin - tabLeftIcon.worldBound.width;
            tabRightIcon.style.left = last.worldBound.xMax;
        }
        else visibility = Visibility.Hidden;

        tabLeftIcon.style.visibility = visibility;
        tabRightIcon.style.visibility = visibility;
    }

    private class DeckTabView {
        public VisualElement container;
        public List<DeckSectionView> sectionViews;
        public DeckSectionView focusedSectionView;
        public DeckTab deckTab;

        public DeckTabView(VisualElement container, DeckTab deckTab) {
            this.container = container;
            this.deckTab = deckTab;
            this.sectionViews = new List<DeckSectionView>();
            List<VisualElement> sectionContainers = container.Q<VisualElement>("SectionsContainer").Children().ToList();
            for (int i = 0; i < sectionContainers.Count; i++) {
                if (i >= deckTab.sections.Count) {
                    sectionContainers[i].style.display = DisplayStyle.None;
                    continue;
                }
                DeckSectionView deckSectionView = new DeckSectionView(sectionContainers[i], deckTab.sections[i]);
                deckSectionView.container.style.display = DisplayStyle.Flex;
                this.sectionViews.Add(deckSectionView);
            }
        }
    }

    private class DeckSectionView {
        public VisualElement container;
        public VisualElement header;
        public VisualElement divider;
        public ScrollView scrollView;
        public CompanionManagementView companionView;
        public DeckTabSection deckTabSection;

        public DeckSectionView(VisualElement container, DeckTabSection deckTabSection) {
            this.container = container;
            this.deckTabSection = deckTabSection;
            this.header = container.Q("SectionHeader");
            this.divider = container.Q("SectionDivider");
            this.scrollView = container.Q<ScrollView>();
        }
    }

    private class TabButton {
        public int index;

        public TabButton(int index) {
            this.index = index;
        }
    }
}

public enum DeckViewType {
    Draw,
    Discard,
    EntireDeck
}