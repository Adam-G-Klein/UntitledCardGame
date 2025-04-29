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

    [Header("VFX")]
    public GameObject moneyGainedPrefab;
    public GameObject moneySpentPrefab;
    public GameObject shopRerollPrefab;
    public GameObject shopUpgradePrefab;

    private ShopEncounter shopEncounter;
    private ShopLevel shopLevel;
    private bool buyingCard = false;
    private bool removingCard = false;
    private CardInShopWithPrice currentCardBuyRequest;
    private ShopItemView currentCardBuyRequestItemView;
    private CompanionCombinationManager companionCombinationManager;
    [SerializeField]
    public UIDocumentGameObjectPlacer placer { get; set; }
    private CompanionInShopWithPrice companionInShop;
    private Companion newCompanion;
    public GameObject tooltipPrefab;

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

    public bool IsApplicableCompanion(CardInShopWithPrice cardInShop, Companion companion) {
        return cardInShop.cardPool == companion.companionType.cardPool ||
            cardInShop.cardPool == companion.companionType.packCardPool ||
            cardInShop.cardPool == shopEncounter.shopData.neutralCardPool;
    }

    public bool IsApplicableCompanionType(CompanionTypeSO cardSourceCompanion, CompanionTypeSO companionType) {
        return cardSourceCompanion == null || cardSourceCompanion.cardPool == companionType.cardPool;
    }


    public void ProcessCompanionBuyRequest(ShopItemView shopItemView, CompanionInShopWithPrice companionInShop) {
        Debug.Log("Processing companion buy request");
        if (DialogueManager.Instance.dialogueInProgress) {
            return;
        }

        if (gameState.playerData.GetValue().gold >= companionInShop.price) {
            // Create a new instance of the companion and then attempt companion upgrades before adding
            // them to your team;
            this.companionInShop = companionInShop;
            newCompanion = new Companion(companionInShop.companionType);
            // companionToAdd is the final companion to add to your team :)
            Companion companionToAdd = newCompanion;

            List<Companion> companions = companionCombinationManager.PurchaseWouldCauseUpgrade(newCompanion);
            if (companions != null) {
                Companion upgradeCompanion = companionCombinationManager.ShowUpgradedCompanion(companions);
                if (companions.Count == 4) {
                    List<Companion> level2s = new()
                    {
                        companions[3],
                        upgradeCompanion
                    };
                    upgradeCompanion = companionCombinationManager.ShowUpgradedCompanion(level2s);
                }
                shopViewController.ShowCompanionUpgradeMenu(companions, upgradeCompanion);
                return;
            }
            gameState.playerData.GetValue().gold -= companionInShop.price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            if (gameState.companions.activeCompanions.Count + shopViewController.blockedSlots.Count == 5 && gameState.companions.benchedCompanions.Count == 5) {
                StartCoroutine(shopViewController.ShowGenericNotification("You have reached the maximum number of companions.", 2));
                return;
            }
            gameState.AddCompanionToTeam(companionToAdd);
            shopViewController.RemoveCompanionFromShopView(companionInShop);
            shopViewController.RebuildUnitManagement(gameState.companions);
            InstantiateShopVFX(moneySpentPrefab, shopItemView.shopItemElement, 1.5f);
        } else {
            Debug.Log("Not enuff munny");
            shopViewController.NotEnoughMoney();
        }
    }

    private void InstantiateShopVFX(GameObject prefab, VisualElement ve, float scale) {
        GameObject instance = Instantiate(prefab, UIDocumentGameObjectPlacer.GetWorldPositionFromElement(ve), Quaternion.identity);
        ScaleGameObjectAndChildren(instance, scale);
    }
    private void ScaleGameObjectAndChildren(GameObject obj, float scale) {
        obj.transform.localScale *= scale;
        foreach (Transform child in obj.transform) {
            child.localScale *= scale;
        }
    }

    public void CancelUpgradePurchase() {
        companionInShop = null;
        newCompanion = null;
    }

    public void ConfirmUpgradePurchase() {
        gameState.playerData.GetValue().gold -= companionInShop.price;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        Companion companionToAdd = null;
        Companion level2Dude = companionCombinationManager.AttemptCompanionUpgrade(newCompanion);
        if (level2Dude != null) {
            companionToAdd = level2Dude;
            // Then attempt the level 3 upgrade :)
            Companion level3Dude = companionCombinationManager.AttemptCompanionUpgrade(level2Dude);
            if (level3Dude != null) companionToAdd = level3Dude;
        }
        gameState.AddCompanionToTeam(companionToAdd);
        shopViewController.RemoveCompanionFromShopView(companionInShop);
        shopViewController.RebuildUnitManagement(gameState.companions);
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
            InstantiateShopVFX(moneySpentPrefab, currentCardBuyRequestItemView.shopItemElement, 1.5f);
        }

        if (removingCard) {
            shopViewController.ShopDeckViewForCardRemoval(companion);
        }
    }

    public void ProcessCardRemoval() {
        gameState.playerData.GetValue().gold -= shopEncounter.shopData.cardRemovalPrice;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        removingCard = false;
    }

    public void ProcessCardBuyCanceled() {
        this.buyingCard = false;
    }

    public void ProcessCardRemovalCancelled() {
        this.buyingCard = false;
    }

    public void SellCompanion(Companion companion, VisualElement ve) {
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
        InstantiateShopVFX(moneyGainedPrefab, ve, 1.1f);
    }

    public int CalculateCompanionSellPrice(Companion companion) {
        int baseSellPrice = 0;
        if (companion.companionType.level == CompanionLevel.LevelOne) {
            baseSellPrice = shopEncounter.shopData.levelOneSellPrice;
        } else if (companion.companionType.level == CompanionLevel.LevelTwo) {
            baseSellPrice = shopEncounter.shopData.levelTwoSellPrice;
        } else if (companion.companionType.level == CompanionLevel.LevelThree) {
            baseSellPrice = shopEncounter.shopData.levelThreeSellPrice;
        }
        baseSellPrice += Convert.ToInt32(companion.deck.cards.Count * shopEncounter.shopData.deckSizeSellFactor);
        return baseSellPrice;
    }

    public void ProcessUpgradeShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        PlayerData playerData = gameState.playerData.GetValue();
        if (playerData.gold >= shopLevel.upgradeCost) {
            // Clean up hoverables for old shop items

            playerData.gold -= shopLevel.upgradeCost;
            playerData.shopLevel += 1;
            shopLevel = shopEncounter.shopData.GetShopLevel(playerData.shopLevel);
            gameState.companions.SetCompanionSlots(shopLevel.teamSize);
            playerData.manaPerTurn = shopLevel.mana;

            shopViewController.SetShopUpgradePrice(shopLevel.upgradeCost);
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            shopViewController.RebuildUnitManagement(gameState.companions);
            InstantiateShopVFX(shopUpgradePrefab, shopViewController.GetUpgradeShopButton(), 1f);
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

    public void ProcessRerollShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= shopEncounter.shopData.rerollShopPrice) {
            gameState.playerData.GetValue().gold -= shopEncounter.shopData.rerollShopPrice;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            shopViewController.Clear();
            InstantiateShopVFX(shopRerollPrefab, shopViewController.GetRerollShopButton(), 1.25f);
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

    public void ProcessCardRemovalClick() {
        removingCard = true;
        // wanna keep this right next to the bools the manager tracks in case we wanna unite state someday
        // "Dalinar... you must UNITE THEM"
        NonMouseInputManager.Instance.SetUIState(UIState.REMOVING_CARD);
        if (gameState.playerData.GetValue().gold >= shopEncounter.shopData.cardRemovalPrice) {
            shopViewController.StartCardRemoval();
        } else {
            shopViewController.NotEnoughMoney();
        }
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

    public bool CanMoveCompanionToNewOpenSlot(Companion companion) {
        if (gameState.companions.activeCompanions.Count == 1 && gameState.companions.activeCompanions.Contains(companion)) {
            return false;
        }
        return true;
    }

    public void ShopItemHovered() {
        MusicController2.Instance.PlaySFX("event:/SFX/SFX_UIHover");
    }

    // To satisfy interface. Unused
    public LocationStore companionLocationStore { get; set; }
    public LocationStore enemyLocationStore { get; set; }

    public void SetRemovingCard(bool val) {
        removingCard = val;
    }

}
