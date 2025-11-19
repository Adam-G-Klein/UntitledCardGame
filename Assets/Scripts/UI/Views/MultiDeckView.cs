using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MultiDeckView
{
    private int currentTabIndex = 0;
    private VisualElement tabsContainer;
    private VisualElement deckViewsContainer;
    public IconButton leftButton { get; private set; }
    public IconButton rightButton { get; private set; }
    public IconButton tabLeftButton { get; private set; }
    public IconButton tabRightButton { get; private set; }
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
    // The following should likely be grouped into a tabData structure
    private List<VisualElement> deckViewTabVisualElements = new List<VisualElement>();
    private List<int> focusedSectionsForEachTab = new List<int>();
    private List<int> numSectionsForEachTab = new List<int>();
    private VisualElement visibilityRoot;
    private IMultiDeckViewDelegate multiDeckViewDelegate;
    private bool inTransition = false;
    private VisualElement tabDescriptorContainer;
    private string TAB_DESCRIPTOR = "tab-descriptor-{0}";
    private bool isEnabled = true;

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

        Tuple<int, int> cardSizeTuple = CardView.GetWidthAndHeight();
        CARD_SIZE = new Vector2(cardSizeTuple.Item1, cardSizeTuple.Item2);
        tabsContainer = uiDocument.rootVisualElement.Q("SectionsContainer");
        visibilityRoot = uiDocument.rootVisualElement.Q("rootElement");
        tabDescriptorContainer = uiDocument.rootVisualElement.Q("tab-descriptor-container");
        ToggleVisibility(false);
        SetupButtons();
    }

    public void PopulateDeckView(List<DeckViewTab> deckViewTabs, int startingTab = 0, int startingIndex = 0)
    {
        currentTabIndex = 0;
        tabDescriptorContainer.DoForAllChildren((element) => element.style.display = DisplayStyle.None);
        for (int i = 0; i < deckViewTabs.Count; i++)
        {
            DeckViewTab tab = deckViewTabs[i];
            deckViewTabVisualElements.Add(tabsContainer.Children().ToList()[i]);
            numSectionsForEachTab.Add(tab.sections.Count);
            focusedSectionsForEachTab.Add(i == startingTab ? startingIndex : 0);
            VisualElement tabContainer = deckViewTabVisualElements[i];
            SetupCards(tab, tabContainer, i == startingTab, i == startingTab ? startingIndex : 0);
            if (i == startingTab)
            {
                MoveInTab(tabContainer, false, true);
                SetTabDescriptorActive(i);
            }
            else
            {
                MoveOutTab(tabContainer, false, true);
            }
            VisualElement tabDescriptor = uiDocument.rootVisualElement.Q<VisualElement>(String.Format(TAB_DESCRIPTOR, i));
            tabDescriptor.style.display = DisplayStyle.Flex;
            tabDescriptor.Q<Label>().text = tab.title;
            // currentTabIndex += 1;
        }
        currentTabIndex = startingTab;

        if (deckViewTabs.Count == 1) {
            uiDocument.rootVisualElement.Q<IconButton>("tabLeftButton").style.visibility = Visibility.Hidden;
            uiDocument.rootVisualElement.Q<IconButton>("tabRightButton").style.visibility = Visibility.Hidden;
        } else {
            uiDocument.rootVisualElement.Q<IconButton>("tabLeftButton").style.visibility = StyleKeyword.Null;
            uiDocument.rootVisualElement.Q<IconButton>("tabRightButton").style.visibility = StyleKeyword.Null;
        }

        //hover first card
        List<VisualElement> focusedDeck = deckViewTabVisualElements[startingTab].Q<VisualElement>("SectionsContainer").Children().ToList()[startingIndex].Q<ScrollView>("ScrollView").contentContainer.Children().ToList();
        if (focusedDeck.Count == 0) return;
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;
        VisualElement firstCard = focusedDeck[0];
        FocusManager.Instance.SetFocus(firstCard.AsFocusable());
    }

    private void SetupButtons()
    {
        // leftButton and rightButton move which companion you're looking at
        // tabLeftButton and tabRightButton move what set of companions / sets
        // of cards you're looking at (active/bench or draw/discard)
        // setup leftButton
        leftButton = uiDocument.rootVisualElement.Q<IconButton>("leftButton");
        leftButton.RegisterOnSelected(LeftButtonClicked);

        leftButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_SECTION_LEFT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_SECTION_LEFT));
        ControlsManager.Instance.RegisterIconChanger(leftButton);

        // setup rightButton
        rightButton = uiDocument.rootVisualElement.Q<IconButton>("rightButton");
        rightButton.RegisterOnSelected(RightButtonClicked);

        rightButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_SECTION_RIGHT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_SECTION_RIGHT));
        ControlsManager.Instance.RegisterIconChanger(rightButton);

        tabLeftButton = uiDocument.rootVisualElement.Q<IconButton>("tabLeftButton");
        tabLeftButton.RegisterOnSelected(TabLeftButtonClicked);

        tabLeftButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_TAB_LEFT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_TAB_LEFT));
        ControlsManager.Instance.RegisterIconChanger(tabLeftButton);
        // setup rightButton
        tabRightButton = uiDocument.rootVisualElement.Q<IconButton>("tabRightButton");
        tabRightButton.RegisterOnSelected(TabRightButtonClicked);

        tabRightButton.SetIcon(
            GFGInputAction.MULTI_DECK_VIEW_TAB_RIGHT,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.MULTI_DECK_VIEW_TAB_RIGHT));
        ControlsManager.Instance.RegisterIconChanger(tabRightButton);

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
        FocusManager.Instance.RegisterFocusableTarget(tabLeftButton.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(tabRightButton.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(exitButton.AsFocusable());
    }

    public void UnregisterFocusables()
    {
        FocusManager.Instance.UnregisterFocusableTarget(leftButton.AsFocusable());
        FocusManager.Instance.UnregisterFocusableTarget(rightButton.AsFocusable());
        FocusManager.Instance.UnregisterFocusableTarget(tabLeftButton.AsFocusable());
        FocusManager.Instance.UnregisterFocusableTarget(tabRightButton.AsFocusable());
        FocusManager.Instance.UnregisterFocusableTarget(exitButton.AsFocusable());
    }

    public void UnFocusDeckSection(VisualElement deckView, bool instant = false)
    {
        LeanTween.value(1f, 0f, instant ? 0f : animationDuration)
        .setOnUpdate((float val) =>
        {
            deckView.style.flexGrow = val;
        })
        .setEase(animationEaseType);

        ToggleDeckFocusability(deckView.Q<ScrollView>("ScrollView").contentContainer.Children().ToList(), false);
        VisualElement companionView = deckView.Q<VisualElement>(className: "companionView");
        if (companionView != null) FocusManager.Instance.DisableFocusableTarget(companionView.AsFocusable());
        SetPositionForUnSelectedSection(deckView, instant);
    }

    public void FocusDeckSection(VisualElement deckView, bool instant = false)
    {
        inTransition = true;
        ScrollView scrollView = deckView.Q<ScrollView>("ScrollView");
        VisualElement companionView = deckView.Q<VisualElement>(className: "companionView");
        LeanTween.value(0f, 1f, instant ? 0f : animationDuration)
            .setOnUpdate((float val) =>
            {
                deckView.style.flexGrow = val;
            })
            .setEase(animationEaseType)
            .setOnComplete(() => {
                OnFocusComplete(scrollView, companionView);
            });
        SetPositionForSelectedSection(scrollView, instant);
    }
    public void OnFocusComplete(ScrollView scrollView, VisualElement companionView)
    {
        // ideally do after a slight delay
        inTransition = false;
        if (scrollView.contentContainer.Children().ToList().Count == 0) return;
        ToggleDeckFocusability(scrollView.contentContainer.Children().ToList(), true);
        if (companionView != null) FocusManager.Instance.EnableFocusableTarget(companionView.AsFocusable());
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;
        VisualElement firstCard = scrollView.contentContainer.Children().ToList()[0];
        FocusManager.Instance.SetFocus(firstCard.AsFocusable());
    }

    private void ToggleDeckFocusability(List<VisualElement> elements, bool enable)
    {
        foreach (VisualElement element in elements)
        {
            if (element.focusable)
            {
                if (enable) FocusManager.Instance.EnableFocusableTarget(element.AsFocusable());
                else FocusManager.Instance.DisableFocusableTarget(element.AsFocusable());
            }
        }
    }

    public void LeftButtonClicked(ClickEvent evt)
    {
        if (inTransition || focusedSectionsForEachTab[currentTabIndex] == 0) return; // remove this line to enable wrap around...I don't think I like it
        UnFocusDeckSection(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList()[focusedSectionsForEachTab[currentTabIndex]], false);
        focusedSectionsForEachTab[currentTabIndex]--;
        FocusDeckSection(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList()[focusedSectionsForEachTab[currentTabIndex]], false);
    }
    public void RightButtonClicked(ClickEvent evt)
    {
        if (inTransition || focusedSectionsForEachTab[currentTabIndex] == numSectionsForEachTab[currentTabIndex] - 1) return; // remove this line to enable wrap around...I don't think I like it
        UnFocusDeckSection(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList()[focusedSectionsForEachTab[currentTabIndex]], false);
        focusedSectionsForEachTab[currentTabIndex]++;
        FocusDeckSection(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList()[focusedSectionsForEachTab[currentTabIndex]], false);
    }

    public void TabLeftButtonClicked(ClickEvent evt)
    {
        //UpdateActiveTab();
        if (inTransition || currentTabIndex == 0) return; // remove this line to enable wrap around...I don't think I like it
        MoveOutTab(deckViewTabVisualElements[currentTabIndex], true);
        int newIndex = (currentTabIndex - 1 + deckViewTabVisualElements.Count) % deckViewTabVisualElements.Count;
        MoveInTab(deckViewTabVisualElements[newIndex], true);
        SetTabDescriptorActive(newIndex);
        currentTabIndex = newIndex;
    }

    public void TabRightButtonClicked(ClickEvent evt)
    {
        //UpdateActiveTab();
        if (inTransition || currentTabIndex == deckViewTabVisualElements.Count - 1) return; // remove this line to enable wrap around...I don't think I like it
        MoveOutTab(deckViewTabVisualElements[currentTabIndex], false);
        ToggleDeckFocusability(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList(), false);
        int newIndex = (currentTabIndex + 1 + deckViewTabVisualElements.Count) % deckViewTabVisualElements.Count;
        MoveInTab(deckViewTabVisualElements[newIndex], false);
        SetTabDescriptorActive(newIndex);
        currentTabIndex = newIndex;
        ToggleDeckFocusability(deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer").Children().ToList(), true);
    }

    public void ExitButtonClicked(ClickEvent evt)
    {
        MoveOutTab(deckViewTabVisualElements[currentTabIndex], true, true);
        ToggleVisibility(false);
        multiDeckViewDelegate.HideDeckView();
        // Cleanup
        CleanUp();
    }

    public void StartScrolling(int direction) {
        GetCurrentScrollView().StartScrolling(0.005f, direction);
    }

    public void StopScrolling() {
        GetCurrentScrollView().StopScrolling();
    }

    private ScrollView GetCurrentScrollView() {
        return deckViewTabVisualElements[currentTabIndex].Q<VisualElement>("SectionsContainer")
            .Children()
            .ToList()[focusedSectionsForEachTab[currentTabIndex]]
            .Q<ScrollView>("ScrollView");
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

    private void SetTabDescriptorActive(int index) {
        for (int i = 0; i < 3; i++) {
            if (i == index) uiDocument.rootVisualElement.Q<VisualElement>(String.Format(TAB_DESCRIPTOR, i)).AddToClassList("tab-descriptor-selected");
            else uiDocument.rootVisualElement.Q<VisualElement>(String.Format(TAB_DESCRIPTOR, i)).RemoveFromClassList("tab-descriptor-selected");
        }
    }

    private void SetupCards(DeckViewTab deckViewTab, VisualElement tabContainer, bool isStartingTab, int startingIndex = 0)
    {
        deckViewsContainer = tabContainer.Q("SectionsContainer");
        for (int i = 0; i < deckViewsContainer.Children().Count(); i++)
        {
            if (i >= deckViewTab.sections.Count)
            {
                deckViewsContainer.Children().ToList()[i].style.display = DisplayStyle.None;
            }
            else
            {
                if (deckViewsContainer.Children().ToList()[i].style.display == DisplayStyle.None) {
                    int index = i;
                    deckViewsContainer.Children().ToList()[i].style.display = DisplayStyle.Flex;
                    EventCallback<GeometryChangedEvent> onGeometryChanged = null;
                    onGeometryChanged = (evt) => {
                        if ((isStartingTab && index == startingIndex) || (!isStartingTab && index == 0)) {
                            FocusDeckSection(deckViewsContainer.Children().ToList()[index], true);
                        }
                        else {
                            UnFocusDeckSection(deckViewsContainer.Children().ToList()[index], true);
                        }
                        (evt.target as VisualElement).UnregisterCallback<GeometryChangedEvent>(onGeometryChanged);
                    };
                    deckViewsContainer.Children().ToList()[i].RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
                }

                deckViewsContainer.Children().ToList()[i].style.display = DisplayStyle.Flex;
                VisualElement sectionContainer = deckViewsContainer.Children().ToList()[i];
                Companion companion = deckViewTab.sections[i].companion;
                VisualTreeAsset companionTemplate = EncounterConstantsSingleton.Instance.encounterConstantsSO.companionManagementViewTemplate;
                CompanionManagementView companionView = new CompanionManagementView(companion, companionTemplate, null);
                VisualElementFocusable companionFocusable = companionView.container.AsFocusable();
                companionView.container.RegisterCallback<PointerEnterEvent>((evt) => {
                    tooltipController.DisplayTooltip(companionView.container, companion.companionType.GetTooltip(), TooltipContext.MultiDeckView);
                });
                companionView.container.RegisterCallback<PointerLeaveEvent>((evt) => {
                    tooltipController.DestroyTooltip(companionView.container);
                });
                companionFocusable.additionalFocusAction += () => {
                    tooltipController.DisplayTooltip(companionView.container, companion.companionType.GetTooltip(), TooltipContext.MultiDeckView);
                };
                companionFocusable.additionalUnfocusAction += () => {
                    tooltipController.DestroyTooltip(companionView.container);
                };
                FocusManager.Instance.RegisterFocusableTarget(companionFocusable);
                companionView.container.AddToClassList("companionView");
                companionView.container.AddToClassList("multi-deck-view-focus-item");
                companionView.UpdateWidthAndHeight(.15f);
                sectionContainer.Q("SectionHeader").Add(companionView.container);

                // Note, this is only valid for during combat. In the shop, we will have no valid combat instance.
                CompanionInstance ci = CombatEntityManager.Instance.GetCompanionInstanceAtPosition(i);

                ScrollView scrollView = sectionContainer.Q<ScrollView>("ScrollView");
                List<Card> cards = deckViewTab.sections[i].cards;
                for (int j = 0; j < cards.Count; j++)
                {
                    Card card = cards[j];
                    CardView cardView = new CardView(card.cardType, companion.companionType, card.shopRarity, true);
                    cardView.cardContainer.AddToClassList("cardView");
                    cardView.cardContainer.AddToClassList("multi-deck-view-focus-item");
                    cardView.cardContainer.MakeFocusable();
                    scrollView.Add(cardView.cardContainer);
                    FocusManager.Instance.RegisterFocusableTarget(cardView.cardFocusable);

                    cardView.cardContainer.RegisterCallback<PointerEnterEvent>((evt) => {
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
                        string modText = card.GetModifiedDescriptionForDeckView(ci.combatInstance);
                        cardView.UpdateCardText(modText);
                    }

                    if (i != startingIndex)
                    {
                        FocusManager.Instance.DisableFocusableTarget(cardView.cardFocusable);
                    }
                }
            }

            if ((isStartingTab && i == startingIndex) || (!isStartingTab && i == 0))
            {
                FocusDeckSection(deckViewsContainer.Children().ToList()[i], true);
            }
            else
            {
                UnFocusDeckSection(deckViewsContainer.Children().ToList()[i], true);
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
            float finalLeft = basePosition.x + X_STEP * (i % CARDS_WIDE_PER_COMP_NUM[numSectionsForEachTab[currentTabIndex]]);
            float finalTop = basePosition.y + Y_STEP * Mathf.Floor(i / CARDS_WIDE_PER_COMP_NUM[numSectionsForEachTab[currentTabIndex]]);
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
        }
    }

    public void CleanUp()
    {
        UnregisterFocusables();
        foreach (VisualElement tab in deckViewTabVisualElements)
        {
            List<VisualElement> sections = tab.Q<VisualElement>("SectionsContainer").Children().ToList();
            foreach (VisualElement section in sections)
            {
                section.Q<VisualElement>("SectionHeader").Clear();
                ScrollView scrollView = section.Q<ScrollView>("ScrollView");
                scrollView.Clear();
                UnFocusDeckSection(section, true);
            }
        }
        deckViewTabVisualElements.Clear();
        focusedSectionsForEachTab.Clear();
        numSectionsForEachTab.Clear();
        tooltipController.DestroyAllTooltips();
    }

    public void TurnOffInteractions() {
        ToggleVisibility(false);
        CleanUp();
        isEnabled = false;
    }
}

public enum DeckViewType {
    Draw,
    Discard,
    EntireDeck
}