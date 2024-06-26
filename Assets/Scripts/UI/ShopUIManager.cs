using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : GenericSingleton<ShopUIManager>
{
    public ShopManager shopManager;
    [Space(10)]
    public TMP_Text playerGoldTMPText;
    [Space(10)]
    public TMP_Text needMoreMoneyTMPText;
    public TMP_Text upgradeShopButtonGoldText;
    public TMP_Text rerollShopButtonGoldText;

    public GameObject companionViewUIPrefab;

    public float needMoreMoneySeconds;
    [TextArea(1,5)]
    public string needMoreMoneyText;
    [Space(10)]
    public Transform cardSection;
    public Transform keepSakeSection;
    [Space(10)]
    public Button upgradeButton;

    void Start() {
        needMoreMoneyTMPText.text = "";
    }

    void Update() {
        playerGoldTMPText.text = shopManager.gameState.playerData
            .GetValue().gold.ToString();
        upgradeShopButtonGoldText.text = shopManager.GetShopLevel().upgradeCost.ToString();
        rerollShopButtonGoldText.text = shopManager.getShopEncounter()
            .shopData.rerollShopPrice.ToString();
    }

    public void DisableUpgradeButton() {
        upgradeButton.interactable = false;
        upgradeShopButtonGoldText.gameObject.SetActive(false);
    }

    public void displayNeedMoreMoneyNotification() {
        StartCoroutine("displayNeedMoreMoneyText");
    }

    IEnumerator displayNeedMoreMoneyText() {
        needMoreMoneyTMPText.text = needMoreMoneyText;
        yield return new WaitForSeconds(needMoreMoneySeconds);
        needMoreMoneyTMPText.text = "";
    }

    public void showCompanionView() {
        GameObject companionViewUI = GameObject.Instantiate(
                        companionViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        companionViewUI
            .GetComponent<CompanionViewUI>()
            .setupCompanionDisplay(shopManager.gameState.companions, new List<CompanionActionType>() {
                CompanionActionType.VIEW_DECK,
                CompanionActionType.MOVE_COMPANION,
                CompanionActionType.COMBINE_COMPANION
            });
    }
}
