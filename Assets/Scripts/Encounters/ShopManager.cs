using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardBuyRequestEventListener))]
public class ShopManager : MonoBehaviour
{
    public bool IS_DEVELOPMENT_MODE = false;
    [Header("Variables")]
    public EncounterVariableSO activeEncounter;
    public MapVariableSO map;
    public CompanionListVariableSO activeCompanions;
    public PlayerDataReference playerData;
    [Header("Shop")]
    public ShopUIManager shopUIManager;
    public EncounterConstants encounterConstants;
    public VoidGameEvent shopRefreshEvent;
    public GameObject companionViewUIPrefab;
    
    private GameObject companionViewUI = null;
    private bool buyingCard = false;
    private CardBuyRequest currentBuyRequest;

    // Start is called before the first frame update
    void Start() {
        if (activeEncounter.Value.getEncounterType() != EncounterType.Shop) {
            Debug.LogError("Active encounter is not a shop but a shop was loaded!");
            return;
        }
        activeEncounter.Value.build(activeCompanions.companionList, encounterConstants);
    }

    void Update() {
        // I want to make it very clear this is frowned upon and is only for testing
        if(IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.R)) {
            shopRefreshEvent.Raise(null);
            ((ShopEncounter) activeEncounter.Value).generateEncounter = true;
            activeEncounter.Value.build(activeCompanions.companionList, encounterConstants);
            ((ShopEncounter) activeEncounter.Value).generateEncounter = false;
        }

        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.G)) {
            playerData.Value.gold += 1;
        }
    }

    public void processCardBuyRequest(CardBuyRequest cardBuyRequest) {
        if (playerData.Value.gold >= cardBuyRequest.price) {
            this.buyingCard = true;
            this.currentBuyRequest = cardBuyRequest;
            this.companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
            this.companionViewUI
                .GetComponent<CompanionViewUI>()
                .setupCompanionDisplay(activeCompanions, new List<CompanionActionType>() {
                    CompanionActionType.SELECT,
                    CompanionActionType.VIEW_DECK
                });
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    public void processCompanionBuyRequest(CompanionBuyRequest request) {
        if (playerData.Value.gold >= request.price) {
            this.activeCompanions.companionBench.Add(request.companion);
            playerData.Value.gold -= request.price;
            GameObject.Instantiate(
                encounterConstants.cardSoldOutPrefab, 
                request.keepsakeInShop.transform.position, 
                Quaternion.identity);
            Destroy(request.keepsakeInShop);
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
            playerData.Value.gold -= currentBuyRequest.price;
            Vector3 cardPosition = currentBuyRequest.cardInShop.transform.position;
            Destroy(currentBuyRequest.cardInShop);
            this.buyingCard = false;
            Destroy(this.companionViewUI);
            this.companionViewUI = null;
            GameObject.Instantiate(
                encounterConstants.cardSoldOutPrefab, 
                cardPosition, 
                Quaternion.identity);
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

    public void exitShop() {
        activeEncounter.Value.isCompleted = true;
        map.GetValue().loadMapScene();
    }
}
