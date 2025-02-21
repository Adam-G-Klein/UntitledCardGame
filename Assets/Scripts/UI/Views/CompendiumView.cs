using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.ComponentModel;
using System.Linq;


// This class isn't expected to have a delegate view or delegate controller because it'll be wrapped
// by one that does
public class CompendiumView {
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
    public CompendiumView(UIDocument uiDocument, CompanionPoolSO companionPool, CardPoolSO neutralCardPool) {
        this.uiDocument = uiDocument;
        cardsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-cards-scrollView");
        companionScrollView = uiDocument.rootVisualElement.Q<ScrollView>("compendium-companions-scrollView");
        cardsScrollView.Clear();
        companionScrollView.Clear();
        SetupCardView(companionPool, neutralCardPool);
        SetupCompanionView(companionPool);
        CardButtonHandler();
        //SetupCardView(uiDocument, companionPool, neutralCardPool);

        // setup buttons
        uiDocument.rootVisualElement.Q<Button>("exitButton").clicked += ExitButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("cardButton").clicked += CardButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("companionButton").clicked += CompanionButtonHandler;
        uiDocument.rootVisualElement.Q<Button>("enemyButton").clicked += EnemyButtonHandler;
    }

    private void EnemyButtonHandler()
    {
        throw new NotImplementedException();
    }

    private void CompanionButtonHandler()
    {
        cardsScrollView.style.display = DisplayStyle.None;
        companionScrollView.style.display = DisplayStyle.Flex;
    }

    private void CardButtonHandler()
    {
        cardsScrollView.style.display = DisplayStyle.Flex;
        companionScrollView.style.display = DisplayStyle.None;
    }

    private void ExitButtonHandler() {
        uiDocument.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    private void SetupCardView(CompanionPoolSO companionPool,CardPoolSO neutralCardPool) {
        cardsSection = new VisualElement();
        cardsSection.AddToClassList("compendium-container");
        List<CompanionTypeSO> companions = companionPool.commonCompanions.Concat(companionPool.uncommonCompanions).Concat(companionPool.rareCompanions).ToList();
        companions.Sort((a, b) => a.companionName.CompareTo(b.companionName)); // we should allow for filtering by rarity or something as well...eventually  
        AddAllCompanionContainers(companions, cardsSection);
        List<CardType> neutralCards = neutralCardPool.commonCards.Concat(neutralCardPool.uncommonCards).Concat(neutralCardPool.rareCards).ToList();
        AddCards(neutralCards, null, cardsSection);
        cardsScrollView.Add(cardsSection);
    }

    private void AddAllCompanionContainers(List<CompanionTypeSO> companions, VisualElement ve) {
        companions.ForEach(companion => {
            List<CardType> cards = new List<CardType>();
            cards = companion.cardPool.commonCards.Concat(companion.cardPool.uncommonCards).Concat(companion.cardPool.rareCards).ToList();
            AddCards(cards, companion, ve);
        });
    }

    private void AddCards(List<CardType> cardType, CompanionTypeSO companion, VisualElement ve) {
        Label companionContainerTitle = new Label();
        companionContainerTitle.text = companion != null ? (companion.companionName + "'s Cards") : "Neutral Cards";
        companionContainerTitle.AddToClassList("compendium-header-text");
        ve.Add(companionContainerTitle);
        
        VisualElement companionCardsContainer = new VisualElement();
        companionCardsContainer.AddToClassList("compendium-section-container");
        cardType.ForEach(card => {
            VisualElement cardContainer = new CardView(card, companion, true).cardContainer;
            cardContainer.AddToClassList("compendium-item-container");
            // add extra styling here to give some padding or shit (should likely be in a common styling)
            companionCardsContainer.Add(cardContainer);
        });
        ve.Add(companionCardsContainer);
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
            companionsToDisplay.ForEach(companionToDisplay => {
                Companion tempCompanion = new Companion(companionToDisplay.companionType);
                EntityView entityView = new EntityView(tempCompanion, 0, false);
                entityView.entityContainer.AddToClassList("compendium-item-container");
                VisualElement portraitContainer = entityView.entityContainer.Q(className: "portrait-container");
                portraitContainer.style.backgroundImage = new StyleBackground(companion.sprite);
                companionRow.Add(entityView.entityContainer);           
            });
            companionsSection.Add(companionRow);
        }
        companionScrollView.Add(companionsSection);
        companionScrollView.style.display = DisplayStyle.None; 
    }
}