using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CardBuyRequestEventListener))]
public class ShopManager : MonoBehaviour
{
    public bool IS_DEVELOPMENT_MODE = false;
    [Header("Variables")]
    public EncounterVariableSO activeEncounter;
    public RoomVariableSO activeRoom;
    public CompanionListVariableSO activeCompanions;
    public PlayerDataReference playerData;
    [Header("Shop")]
    public ShopUIManager shopUIManager;
    public EncounterConstants encounterConstants;
    public VoidGameEvent shopRefreshEvent;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(activeRoom.GetValue().id);
        if (activeEncounter.Value.getEncounterType() != EncounterType.Shop) {
            Debug.LogError("Active encounter is not a shop but a shop was loaded!");
            return;
        }
        activeEncounter.Value.build(encounterConstants);
    }

    void Update() {
        // I want to make it very cleat this is frowned upon and is only for testing
        if(IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.R)) {
            shopRefreshEvent.Raise(null);
            ((ShopEncounter) activeEncounter.Value).generateEncounter = true;
            activeEncounter.Value.build(encounterConstants);
            ((ShopEncounter) activeEncounter.Value).generateEncounter = false;
        }

        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.G)) {
            playerData.Value.gold += 1;
        }
    }

    public void processBuyRequest(CardBuyRequest cardBuyRequest) {
        if (playerData.Value.gold >= cardBuyRequest.price) {
            // Add the card to the minion (how decide what minion???)
            Debug.Log("Card (not) added to minion");
            playerData.Value.gold -= cardBuyRequest.price;
            Destroy(cardBuyRequest.cardInShop);
        } else {
            shopUIManager.displayNeedMoreMoneyNotification();
        }
    }

    public void exitShop() {
        activeEncounter.Value.isCompleted = true;
        SceneManager.LoadScene("PlaceholderRoom");
    }
}
