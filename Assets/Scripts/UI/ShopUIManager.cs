using System.Collections;
using UnityEngine;
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
    public float needMoreMoneySeconds;
    [TextArea(1,5)]
    public string needMoreMoneyText;

    public Transform cardSection;
    public Transform keepSakeSection;

    void Start() {
        needMoreMoneyTMPText.text = "";
    }

    void Update() {
        playerGoldTMPText.text = shopManager.activePlayerDataVariable
            .GetValue().gold.ToString();
        upgradeShopButtonGoldText.text = shopManager.getShopEncounter()
            .shopData.upgradeShopPrice.ToString();
        rerollShopButtonGoldText.text = shopManager.getShopEncounter()
            .shopData.rerollShopPrice.ToString();
    }

    public void displayNeedMoreMoneyNotification() {
        StartCoroutine("displayNeedMoreMoneyText");
    }

    IEnumerator displayNeedMoreMoneyText() {
        needMoreMoneyTMPText.text = needMoreMoneyText;
        yield return new WaitForSeconds(needMoreMoneySeconds);
        needMoreMoneyTMPText.text = "";
    }
}
