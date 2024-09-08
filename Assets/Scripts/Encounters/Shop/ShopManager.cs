using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : GenericSingleton<ShopManager>, IEncounterBuilder
{
    public bool IS_DEVELOPMENT_MODE = false;

    [Header("Variables")]
    public GameStateVariableSO gameState;

    [Header("Shop")]
    public ShopUIManager shopUIManager;
    public EncounterConstantsSO encounterConstants;
    public VoidGameEvent shopRefreshEvent;
    public GameObject companionViewUIPrefab;

    private ShopEncounter shopEncounter;
    private ShopLevel shopLevel;
    private GameObject companionViewUI = null;
    private bool buyingCard = false;
    private CardBuyRequest currentBuyRequest;
    private CompanionCombinationManager companionCombinationManager;
    [SerializeField]
    public UIDocumentGameObjectPlacer placer { get; set; }

    void Awake() {
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
        shopEncounter.Build(this, allCompanions, encounterConstants, this.shopLevel);

        CheckDisableUpgradeButton();
        /* uncomment to re-enable shop dialogue
        DialogueManager.Instance.SetDialogueLocation(
            gameState.dialogueLocations.GetDialogueLocation(gameState));
        DialogueManager.Instance.StartAnyDialogueSequence();
        */
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
            if(!companionCombinationManager.AttemptUpgradeCompanion(request.companion)){
                Debug.Log("Upgrade not appicable, adding to benched companions");
                this.gameState.companions.benchedCompanions.Add(request.companion);
            }
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
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

    private void CheckDisableUpgradeButton() {
        if (shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level) {
            shopUIManager.DisableUpgradeButton();
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

    private void rerollShop() {
        shopRefreshEvent.Raise(null);
        List<Companion> allCompanions = new();
        allCompanions.AddRange(gameState.companions.activeCompanions);
        allCompanions.AddRange(gameState.companions.benchedCompanions);
        shopEncounter.Build(this, allCompanions, encounterConstants, shopLevel);
    }

    public void saveShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
    }

    public void exitShop() {
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

    // To satisfy interface. Unused
    public LocationStore companionLocationStore { get; set; }
    public LocationStore enemyLocationStore { get; set; }
}
