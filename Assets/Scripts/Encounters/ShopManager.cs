using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class ShopManager : GenericSingleton<ShopManager>
{
    public bool IS_DEVELOPMENT_MODE = false;

    [Header("Variables")]
    public EncounterVariableSO activeEncounterVariable;
    public MapVariableSO mapVariable;
    public CompanionListVariableSO activeCompanionsVariable;
    public PlayerDataVariableSO activePlayerDataVariable;

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

    // Start is called before the first frame update
    void Start() {
        if (activeEncounterVariable.GetValue().getEncounterType() != EncounterType.Shop) {
            Debug.LogError("Active encounter is not a shop but a shop was loaded!");
            return;
        }
        activeEncounterVariable.GetValue().build(activeCompanionsVariable.companionList, encounterConstants);
        shopLevel = shopEncounter.shopData.GetShopLevel(activePlayerDataVariable.GetValue().shopLevel);
    }

    void Update() {
        // I want to make it very clear this is frowned upon and is only for testing
        if(IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.R)) {
            rerollShop();
        }

        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.G)) {
            activePlayerDataVariable.GetValue().gold += 1;
        }
    }

    public void processCardBuyRequest(CardBuyRequest cardBuyRequest) {
        if (activePlayerDataVariable.GetValue().gold >= cardBuyRequest.price) {
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
                activeCompanionsVariable.currentCompanionSlots, 
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
        foreach (var companion in activeCompanionsVariable.companionList) {
            //Now this is Epic
            if (companion.companionType.cardPool.commonCards.Contains(cardInfo.cardType) ||
                companion.companionType.cardPool.uncommonCards.Contains(cardInfo.cardType) ||
                companion.companionType.cardPool.rareCards.Contains(cardInfo.cardType)) {
                companionList.Add(companion);
            }
        }

        return companionList;
    }

    private List<Companion> determineApplicableBenchCompanions(Card cardInfo) {
        
        List<Companion> companionList = new();

        //set up bench list
        foreach (var companion in activeCompanionsVariable.companionBench) {
            if (companion.companionType.cardPool.commonCards.Contains(cardInfo.cardType) ||
                companion.companionType.cardPool.uncommonCards.Contains(cardInfo.cardType) ||
                companion.companionType.cardPool.rareCards.Contains(cardInfo.cardType)) {
                companionList.Add(companion);
            }
        }

        return companionList;
    }

    public void processCompanionBuyRequest(CompanionBuyRequest request) {
        if (activePlayerDataVariable.GetValue().gold >= request.price) {
            this.activeCompanionsVariable.companionBench.Add(request.companion);
            activePlayerDataVariable.GetValue().gold -= request.price;
            GameObject.Instantiate(
                encounterConstants.cardSoldOutPrefab, 
                request.keepsakeInShop.transform.position, 
                Quaternion.identity);
            request.keepsakeInShop.sold();
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
            companion.deck.cards.Add(currentBuyRequest.cardInfo);
            activePlayerDataVariable.GetValue().gold -= currentBuyRequest.price;
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
        if (activePlayerDataVariable.GetValue().gold >= shopLevel.upgradeCost) {
            activePlayerDataVariable.GetValue().gold -= shopLevel.upgradeCost;
            activeCompanionsVariable.SetCompanionSlots(shopLevel.teamSize);
            // Handle mana increase
            activePlayerDataVariable.GetValue().shopLevel += 1;
            shopLevel = shopEncounter.shopData.GetShopLevel(activePlayerDataVariable.GetValue().shopLevel);

        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    // Attached as a UnityEvent to the RerollShop button
    public void processRerollShopClick() {
        if (activePlayerDataVariable.GetValue().gold >= shopEncounter.shopData.rerollShopPrice) {
            activePlayerDataVariable.GetValue().gold -= shopEncounter.shopData.rerollShopPrice;
            rerollShop();
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    private void rerollShop() {
        shopRefreshEvent.Raise(null);
        activeEncounterVariable.GetValue().build(activeCompanionsVariable.companionList, encounterConstants);
    }

    public void saveShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
    }

    public void exitShop() {
        activeEncounterVariable.GetValue().isCompleted = true;
        mapVariable.GetValue().loadMapScene();
    }

    public ShopEncounter getShopEncounter() {
        return this.shopEncounter;
    }

    public ShopLevel GetShopLevel() {
        return shopLevel;
    }
}
