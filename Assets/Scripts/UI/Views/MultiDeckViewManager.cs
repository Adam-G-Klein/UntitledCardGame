using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using OpenCover.Framework.Model;
using Unity.VisualScripting;

public class DeckViewTab
{
    public string title;
    public List<DeckViewTabSection> sections;
}

public class DeckViewTabSection
{
    public Companion companion;
    public List<Card> cards;
}

public class MultiDeckViewManager : GenericSingleton<MultiDeckViewManager>, IMultiDeckViewDelegate, IControlsReceiver
{
    private MultiDeckView multiDeckView;
    private bool isVisible = false;
    [SerializeField]
    private GameStateVariableSO gameState;
    private Canvas canvas;
    private Camera mainCamera;

    public delegate void EnterExitVoidHandler();
    private event EnterExitVoidHandler onViewEnterHandler;
    private event EnterExitVoidHandler onViewExitHandler;
    private bool optionsMenuOpen = false;

    public enum TabType
    {
        Active = 0,
        Bench = 1,
        ForPurchase = 2
    }

    public void Start()
    {
        multiDeckView = new MultiDeckView(this, GetComponent<UIDocument>(), GetComponent<CanvasGroup>());
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
        OptionsViewController.Instance.SetEnterHandler(() => optionsMenuOpen = true);
        OptionsViewController.Instance.SetExitHandler(() => optionsMenuOpen = false);
    }

    public void OnViewEnter()
    {
        isVisible = true;
        ControlsManager.Instance.RegisterControlsReceiver(this);
        UpdateCameraReference();
        onViewEnterHandler?.Invoke();
    }

    public void ShowView(List<DeckViewTab> deckViewTabs = null, int startingTab = 0, int startingIndex = 0)
    {
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        multiDeckView.PopulateDeckView(deckViewTabs, startingTab, startingIndex);
        multiDeckView.ToggleVisibility(true);
        multiDeckView.RegisterFocusables();
        FocusManager.Instance.LockFocusables();
    }
    public void ShowCombatDeckView(CompanionInstance companionInstance = null, int startingTab = 0)
    {
        if (isVisible)
        {
            return;
        }
        if (!CombatEntityManager.Instance)
        {
            Debug.LogError("No CombatEntityManager found in scene, cannot show combat deck view");
            return;
        }
        OnViewEnter();
        int startingIndex = companionInstance == null ? 0 : CombatEntityManager.Instance.getCompanions().Where(c => c != null).ToList().IndexOf(companionInstance);

        List<DeckViewTab> deckViewTabs = new List<DeckViewTab>();
        DeckViewTab combatDeckViewTab = new DeckViewTab
        {
            title = "Draw",
            sections = new List<DeckViewTabSection>()
        };
        List<CompanionInstance> activeCompanions = CombatEntityManager.Instance.getCompanions().Where(c => c != null).ToList();
        for (int i = 0; i < activeCompanions.Count; i++)
        {
            DeckViewTabSection section = new DeckViewTabSection
            {
                companion = activeCompanions[i].companion,
                cards = activeCompanions[i].deckInstance.drawPile
            };
            combatDeckViewTab.sections.Add(section);
        }
        deckViewTabs.Add(combatDeckViewTab);

        DeckViewTab discardDeckViewTab = new DeckViewTab
        {
            title = "Discard",
            sections = new List<DeckViewTabSection>()
        };
        for (int i = 0; i < activeCompanions.Count; i++)
        {
            Companion companion = activeCompanions[i].companion;
            DeckViewTabSection section = new DeckViewTabSection
            {
                companion = companion,
                cards = activeCompanions[i].deckInstance.discardPile
            };
            discardDeckViewTab.sections.Add(section);
        }
        deckViewTabs.Add(discardDeckViewTab);
        ShowView(deckViewTabs, startingTab, startingIndex);
    }

    public void ShowShopDeckView(bool showCompanionForPurchase = false, Companion companionToShow = null, TabType startingTab = TabType.Active)
    {
        if (isVisible)
        {
            return;
        }
        if (!ShopManager.Instance)
        {
            Debug.LogError("No ShopManager found in scene, cannot show shop combat deck view");
            return;
        }
        OnViewEnter();

        List<Companion> activeCompanions = gameState.companions.activeCompanions;
        List<DeckViewTab> shopDeckViewTabs = new List<DeckViewTab>();
        bool noActiveCompanions = activeCompanions.Count == 0;
        if (!noActiveCompanions)
        {
            DeckViewTab activeDeckTab = new DeckViewTab
            {
                title = "Active",
                sections = new List<DeckViewTabSection>()
            };
            for (int i = 0; i < activeCompanions.Count; i++)
            {
                DeckViewTabSection section = new DeckViewTabSection
                {
                    companion = activeCompanions[i],
                    cards = activeCompanions[i].deck.cards
                };
                activeDeckTab.sections.Add(section);
            }
            shopDeckViewTabs.Add(activeDeckTab);
        }

        List<Companion> benchCompanions = gameState.companions.benchedCompanions;
        if (benchCompanions.Count != 0)
        {
            DeckViewTab benchDeckViewTab = new DeckViewTab
            {
                title = "Bench",
                sections = new List<DeckViewTabSection>()
            };
            for (int i = 0; i < benchCompanions.Count; i++)
            {
                Companion companion = benchCompanions[i];
                DeckViewTabSection section = new DeckViewTabSection
                {
                    companion = companion,
                    cards = companion.deck.cards
                };
                benchDeckViewTab.sections.Add(section);
            }
            shopDeckViewTabs.Add(benchDeckViewTab);
        }

        if (showCompanionForPurchase)
        {
            DeckViewTab companionForPurchaseDeckViewTabs = new DeckViewTab
            {
                title = "For Purchase",
                sections = new List<DeckViewTabSection>()
            };

            DeckViewTabSection companionForPurchase = new DeckViewTabSection
            {
                companion = companionToShow,
                cards = companionToShow.deck.cards
            };
            companionForPurchaseDeckViewTabs.sections.Add(companionForPurchase);
            shopDeckViewTabs.Add(companionForPurchaseDeckViewTabs);
        }
        int startingIndex = 0;
        if (!showCompanionForPurchase)
        {
            if (startingTab == TabType.Active)
            {
                startingIndex = companionToShow == null ? 0 : activeCompanions.IndexOf(companionToShow);
            }
            else
            {
                startingIndex = companionToShow == null ? 0 : benchCompanions.IndexOf(companionToShow);
            }
        }
        int startingTabIndex = 0;
        if (startingTab == TabType.Bench) startingTabIndex = noActiveCompanions ? 0 : 1;
        if (startingTab == TabType.ForPurchase) startingTabIndex = shopDeckViewTabs.Count - 1;

        ShowView(shopDeckViewTabs, startingTabIndex, startingIndex);
    }

    public void HideDeckView()
    {
        isVisible = false;
        FocusManager.Instance.UnlockFocusables();
        multiDeckView.UnregisterFocusables();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        ControlsManager.Instance.UnregisterControlsReceiver(this);
        onViewExitHandler?.Invoke();
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (optionsMenuOpen) return;
        if (action == GFGInputAction.BACK)
        {
            multiDeckView.ExitButtonClicked(multiDeckView.exitButton.CreateFakeClickEvent());
        }
        if (action == GFGInputAction.MULTI_DECK_VIEW_TAB_LEFT)
        {
            multiDeckView.TabLeftButtonClicked(multiDeckView.leftButton.CreateFakeClickEvent());
        }
        if (action == GFGInputAction.MULTI_DECK_VIEW_TAB_RIGHT)
        {
            multiDeckView.TabRightButtonClicked(multiDeckView.rightButton.CreateFakeClickEvent());
        }
        if (action == GFGInputAction.MULTI_DECK_VIEW_SECTION_LEFT)
        {
            multiDeckView.LeftButtonClicked(multiDeckView.tabLeftButton.CreateFakeClickEvent());
        }
        if (action == GFGInputAction.MULTI_DECK_VIEW_SECTION_RIGHT)
        {
            multiDeckView.RightButtonClicked(multiDeckView.tabRightButton.CreateFakeClickEvent());
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        // just had to be here tbh
    }

    private void UpdateCameraReference()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Debug.Log("Main camera updated: " + mainCamera.name);
        }
        else
        {
            Debug.LogWarning("Main camera not found");
        }
        canvas.worldCamera = mainCamera;
    }
    
    public void SetEnterHandler(EnterExitVoidHandler handler) {
        onViewEnterHandler += handler;
    }

    public void SetExitHandler(EnterExitVoidHandler handler) {
        onViewExitHandler += handler;
    }
}

public interface IMultiDeckViewDelegate
{
    void HideDeckView();
}