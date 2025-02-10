using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : GenericSingleton<ShopManager>, IEncounterBuilder
{
    public bool IS_DEVELOPMENT_MODE = false;
    public bool USE_NEW_SHOP = false;

    [Header("Variables")]
    public GameStateVariableSO gameState;

    [Header("Shop")]
    public ShopUIManager shopUIManager;
    public ShopViewController shopViewController;
    public EncounterConstantsSO encounterConstants;
    public VoidGameEvent shopRefreshEvent;
    public GameObject companionViewUIPrefab;

    private ShopEncounter shopEncounter;
    private ShopLevel shopLevel;
    private GameObject companionViewUI = null;
    private bool buyingCard = false;
    // Old one
    private CardBuyRequest currentBuyRequest;
    // New one, delete old one once migration is finished
    private CardInShopWithPrice currentCardBuyRequest;
    private ShopItemView currentCardBuyRequestItemView;
    private CompanionCombinationManager companionCombinationManager;
    [SerializeField]
    public UIDocumentGameObjectPlacer placer { get; set; }

    void Awake() {
        if (USE_NEW_SHOP) {
            gameObject.GetComponent<UIDocument>().enabled = true;
            shopUIManager.gameObject.SetActive(false);
            shopViewController.Init(this);
        } else {
            gameObject.GetComponent<UIDocument>().enabled = false;
            shopUIManager.gameObject.SetActive(true);
        }
        // This ends up calling BuildShopEncounter below
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
    }

    void Start() {
        companionCombinationManager = GetComponent<CompanionCombinationManager>();
    }

    public void BuildShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
        this.shopLevel = shopEncounter.shopData.GetShopLevel(gameState.playerData.GetValue().shopLevel);
        List<Companion> allCompanions = new();
        allCompanions.AddRange(gameState.companions.activeCompanions);
        allCompanions.AddRange(gameState.companions.benchedCompanions);
        shopEncounter.Build(this, allCompanions, encounterConstants, this.shopLevel, USE_NEW_SHOP);
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        shopViewController.SetShopUpgradePrice(shopLevel.upgradeCost);
        shopViewController.SetShopRerollPrice(shopEncounter.shopData.rerollShopPrice);

        CheckDisableUpgradeButton();
        CheckDisableUpgradeButtonV2();
        /* uncomment to re-enable shop dialogue
        DialogueManager.Instance.SetDialogueLocation(
            gameState.dialogueLocations.GetDialogueLocation(gameState));
        DialogueManager.Instance.StartAnyDialogueSequence();
        */
    }

    public void SetupUnitManagement() {
        StartCoroutine(DelayedSetupUnitManagement());
    }

    private IEnumerator DelayedSetupUnitManagement() {
        yield return new WaitForEndOfFrame();
        shopViewController.SetupUnitManagement(gameState.companions);
    }

    void Update() {
        // I want to make it very clear this is frowned upon and is only for testing
        if(IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.R)) {
            rerollShop();
        }

        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.G)) {
            gameState.playerData.GetValue().gold += 1;
        }
    }

    public void ProcessCardBuyRequestV2(ShopItemView shopItemView, CardInShopWithPrice cardInShop) {
        if (DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= cardInShop.price) {
            this.buyingCard = true;
            this.currentCardBuyRequest = cardInShop;
            this.currentCardBuyRequestItemView = shopItemView;
            shopViewController.CardBuyingSetup(shopItemView, cardInShop);
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    public void processCardBuyRequest(CardBuyRequest cardBuyRequest) {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= cardBuyRequest.price) {
            this.buyingCard = true;
            this.currentBuyRequest = cardBuyRequest;
            this.companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);

            this.companionViewUI
                .GetComponent<CompanionViewUI>()
                .setupCompanionDisplay(determineApplicableActiveCompanions(cardBuyRequest.cardInfo),
                determineApplicableBenchCompanions(cardBuyRequest.cardInfo),
                gameState.companions.currentCompanionSlots,
                new List<CompanionActionType>() {
                    CompanionActionType.SELECT,
                    CompanionActionType.VIEW_DECK
                });
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    private List<Companion> determineApplicableActiveCompanions(Card cardInfo) {
        List<Companion> companionList = new();

        //set up companion list
        foreach (var companion in gameState.companions.activeCompanions) {
            CompanionTypeSO cardSourceCompanion = cardInfo.getCompanionFrom();
            // Source companion will be null if the card is neutral; in that case, we should include all
            // companions.
            // Also, do it based on card pool so the same card can be assigned to a
            // level 1 or level 2 companion of the same type.
            if (cardSourceCompanion == null || cardSourceCompanion.cardPool == companion.companionType.cardPool) {
                companionList.Add(companion);
            }
        }

        return companionList;
    }

    private List<Companion> determineApplicableBenchCompanions(Card cardInfo) {

        List<Companion> companionList = new();

        //set up bench list
        foreach (var companion in gameState.companions.benchedCompanions) {
            CompanionTypeSO cardSourceCompanion = cardInfo.getCompanionFrom();
            // Source companion will be null if the card is neutral; in that case, we should include all
            // companions.
            // Also, do it based on card pool so the same card can be assigned to a
            // level 1 or level 2 companion of the same type.
            if (cardSourceCompanion == null || cardSourceCompanion.cardPool == companion.companionType.cardPool) {
                companionList.Add(companion);
            }
        }

        return companionList;
    }

    public bool IsApplicableCompanion(CompanionTypeSO cardSourceCompanion, Companion companion) {
        return cardSourceCompanion == null || cardSourceCompanion.cardPool == companion.companionType.cardPool;
    }

    public void ProcessCompanionBuyRequestV2(ShopItemView shopItemView, CompanionInShopWithPrice companionInShop) {
        if (gameState.companions.activeCompanions.Count == 5 && gameState.companions.benchedCompanions.Count == 5) {
            StartCoroutine(shopViewController.ShowGenericNotification("You have reached the maximum number of companions.", 2));
            return;
        }
        Debug.Log("Processing companion buy request");
        if (DialogueManager.Instance.dialogueInProgress) {
            return;
        }

        if (gameState.playerData.GetValue().gold >= companionInShop.price) {
            gameState.playerData.GetValue().gold -= companionInShop.price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            // Create a new instance of the companion and then attempt companion upgrades before adding
            // them to your team;
            Companion newCompanion = new Companion(companionInShop.companionType);
            // companionToAdd is the final companion to add to your team :)
            Companion companionToAdd = newCompanion;
            Companion level2Dude = companionCombinationManager.AttemptCompanionUpgrade(newCompanion);
            if (level2Dude != null) {
                companionToAdd = level2Dude;
                // Then attempt the level 3 upgrade :)
                Companion level3Dude = companionCombinationManager.AttemptCompanionUpgrade(level2Dude);

                if (level3Dude != null) {
                    companionToAdd = level3Dude;
                    shopViewController.ShowCompanionUpgradedMessage(newCompanion.GetName(), 3);
                } else {
                    shopViewController.ShowCompanionUpgradedMessage(newCompanion.GetName(), 2);
                }
            }
            gameState.AddCompanionToTeam(companionToAdd);
            shopViewController.RemoveCompanionFromShopView(companionInShop);
            shopViewController.RebuildUnitManagement(gameState.companions);
        } else {
            Debug.Log("Not enuff munny");
            shopViewController.NotEnoughMoney();
        }
    }

    public void processCompanionBuyRequest(CompanionBuyRequest request) {
        Debug.Log("Processing companion buy request");
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }

        if (gameState.playerData.GetValue().gold >= request.price) {
            gameState.playerData.GetValue().gold -= request.price;
            // check that this wasn't purchased with debug tooling
            if(request.keepsakeInShop != null) {
                GameObject.Instantiate(
                encounterConstants.cardSoldOutPrefab,
                request.keepsakeInShop.transform.position,
                Quaternion.identity);
                request.keepsakeInShop.sold();
            }

            // guyToAdd is the final companion to add to your team :)
            Companion companionToAdd = request.companion;
            Companion level2Dude = companionCombinationManager.AttemptCompanionUpgrade(request.companion);
            if (level2Dude != null) {
                companionToAdd = level2Dude;
                // Then attempt the level 3 upgrade :)
                Companion level3Dude = companionCombinationManager.AttemptCompanionUpgrade(level2Dude);

                if (level3Dude != null) {
                    companionToAdd = level3Dude;
                }
            }
            gameState.AddCompanionToTeam(companionToAdd);
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    public void ProcessCompanionClicked(Companion companion) {
        // The player selected a companion, so the transaction is complete
        // (assuming there is a transaction) and we're gonna add the card
        // to the companion's deck and lets forcefully close the companion
        // view UI
        if (this.buyingCard) {
            Card newCard = new Card(currentCardBuyRequest.cardType, companion.companionType, currentCardBuyRequest.rarity);
            companion.deck.cards.Add(newCard);
            gameState.playerData.GetValue().gold -= currentCardBuyRequest.price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            shopViewController.RemoveCardFromShopView(currentCardBuyRequestItemView.cardInShop);
            this.buyingCard = false;
            shopViewController.StopBuyingCard();
        }
    }

    public void ProcessCardBuyCanceled() {
        this.buyingCard = false;
    }

    public void SellCompanion(Companion companion) {
        if (gameState.companions.activeCompanions.Count + gameState.companions.benchedCompanions.Count == 1) {
            shopViewController.ShowCantSellLastCompanion();
            return;
        }
        int sellValue = CalculateCompanionSellPrice(companion);
        gameState.playerData.GetValue().gold += sellValue;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        gameState.companions.activeCompanions.Remove(companion);
        gameState.companions.benchedCompanions.Remove(companion);
        shopViewController.RebuildUnitManagement(gameState.companions);
    }

    public int CalculateCompanionSellPrice(Companion companion) {
        return shopEncounter.shopData.companionKeepsakePrice / 2 + companion.deck.cards.Count / 5 * shopEncounter.shopData.cardPrice;
    }

    public void processCompanionSelectedEvent(Companion companion) {
        // The player selected a companion, so the transaction is complete
        // (assuming there is a transaction) and we're gonna add the card
        // to the companion's deck and lets forcefully close the companion
        // view UI
        if (this.buyingCard) {
            currentBuyRequest.cardInfo.setCompanionFrom(companion.companionType);
            companion.deck.cards.Add(currentBuyRequest.cardInfo);
            gameState.playerData.GetValue().gold -= currentBuyRequest.price;
            Vector3 cardPosition = currentBuyRequest.cardInShop.transform.position;
            currentBuyRequest.cardInShop.GetComponent<CardInShop>().sold();
            this.buyingCard = false;
            Destroy(this.companionViewUI);
            this.companionViewUI = null;
        } else {
            Debug.LogError("Processing companion click event with no transaction");
        }
    }

    public void processCompanionViewExitedEvent() {
        // The player did not assign a companion to add the card to
        // so stop buying a card
        this.buyingCard = false;

        // choosing to null this is hopes that if we see an NPE here then we know
        // something went wrong;
        this.currentBuyRequest = null;
    }

    // Attached as a UnityEvent to the UpgradeShop button
    public void processUpgradeShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        PlayerData playerData = gameState.playerData.GetValue();
        if (playerData.gold >= shopLevel.upgradeCost) {
            playerData.gold -= shopLevel.upgradeCost;
            playerData.shopLevel += 1;
            shopLevel = shopEncounter.shopData.GetShopLevel(playerData.shopLevel);
            gameState.companions.SetCompanionSlots(shopLevel.teamSize);
            playerData.manaPerTurn = shopLevel.mana;

            shopUIManager.RefreshUpgradeButtonTooltip();
            CheckDisableUpgradeButton();
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    public void ProcessUpgradeShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        PlayerData playerData = gameState.playerData.GetValue();
        if (playerData.gold >= shopLevel.upgradeCost) {
            playerData.gold -= shopLevel.upgradeCost;
            playerData.shopLevel += 1;
            shopLevel = shopEncounter.shopData.GetShopLevel(playerData.shopLevel);
            gameState.companions.SetCompanionSlots(shopLevel.teamSize);
            playerData.manaPerTurn = shopLevel.mana;

            shopViewController.SetShopUpgradePrice(shopLevel.upgradeCost);
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            shopViewController.RebuildUnitManagement(gameState.companions);
            CheckDisableUpgradeButtonV2();
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    private void CheckDisableUpgradeButton() {
        if (shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level) {
            shopUIManager.DisableUpgradeButton();
        }
    }

    private void CheckDisableUpgradeButtonV2() {
        if (shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level) {
            shopViewController.DisableUpgradeButton();
        }
    }

    // Attached as a UnityEvent to the RerollShop button
    public void processRerollShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= shopEncounter.shopData.rerollShopPrice) {
            gameState.playerData.GetValue().gold -= shopEncounter.shopData.rerollShopPrice;
            rerollShop();
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    public void ProcessRerollShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= shopEncounter.shopData.rerollShopPrice) {
            gameState.playerData.GetValue().gold -= shopEncounter.shopData.rerollShopPrice;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            shopViewController.Clear();
            rerollShop();
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    private void rerollShop() {
        shopRefreshEvent.Raise(null);
        List<Companion> allCompanions = new();
        allCompanions.AddRange(gameState.companions.activeCompanions);
        allCompanions.AddRange(gameState.companions.benchedCompanions);
        shopEncounter.Build(this, allCompanions, encounterConstants, shopLevel, USE_NEW_SHOP);
    }

    public void saveShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
    }

    public void exitShop() {
        if (gameState.companions.activeCompanions.Count == 0) {
            StartCoroutine(shopViewController.ShowGenericNotification("You need at least one active companion!"));
            return;
        }
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("gameState active encounter id: " + gameState.activeEncounter.GetValue().id);
        Debug.Log("gameState active encounter complete: " + gameState.activeEncounter.GetValue().isCompleted);
        gameState.LoadNextLocation();
    }

    public ShopEncounter getShopEncounter() {
        return this.shopEncounter;
    }

    public ShopLevel GetShopLevel() {
        return shopLevel;
    }

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildEnemyEncounter(EnemyEncounter encounter, UIDocumentGameObjectPlacer placer) {
        Debug.LogError("The shop encounter scene was loaded but the active encounter is an enemy encounter!");
        return;
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToActiveAtIndex(Companion companion, int index) {
        if (gameState.companions.activeCompanions.Contains(companion)) {
            gameState.companions.activeCompanions.Remove(companion);
        }
        gameState.companions.activeCompanions.Add(companion);
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToBenchAtIndex(Companion companion, int index) {
        if (gameState.companions.benchedCompanions.Contains(companion)) {
            gameState.companions.benchedCompanions.Remove(companion);
        }
        gameState.companions.benchedCompanions.Add(companion);
    }

    public void SetCompanionOrdering(List<Companion> activeCompanions, List<Companion> benchCompanions) {
        gameState.companions.activeCompanions = activeCompanions;
        gameState.companions.benchedCompanions = benchCompanions;
    }

    public bool CanMoveCompanionToNewOpenSlot(Companion companion)
    {
        if (gameState.companions.activeCompanions.Count == 1 && gameState.companions.activeCompanions.Contains(companion)) {
            return false;
        }
        return true;
    }

    // To satisfy interface. Unused
    public LocationStore companionLocationStore { get; set; }
    public LocationStore enemyLocationStore { get; set; }
}
