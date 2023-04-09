using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    public PlayerDataVariableSO playerDataVariable;
    [Space(10)]
    public TMP_Text playerGoldTMPText;
    [Space(10)]
    public TMP_Text needMoreMoneyTMPText;
    public TMP_Text upgradeShopButtonGoldText;
    public TMP_Text rerollShopButtonGoldText;
    public float needMoreMoneySeconds;
    [TextArea(1,5)]
    public string needMoreMoneyText;

    private ShopEncounter shopEncounter;

    void Start() {
        needMoreMoneyTMPText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        playerGoldTMPText.text = playerDataVariable.GetValue().gold.ToString();
        upgradeShopButtonGoldText.text = shopEncounter.shopData.upgradeShopPrice.ToString();
        rerollShopButtonGoldText.text = shopEncounter.shopData.rerollShopPrice.ToString();
    }

    public void displayNeedMoreMoneyNotification() {
        StartCoroutine("displayNeedMoreMoneyText");
    }

    public void saveShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
    }

    IEnumerator displayNeedMoreMoneyText() {
        needMoreMoneyTMPText.text = needMoreMoneyText;
        yield return new WaitForSeconds(needMoreMoneySeconds);
        needMoreMoneyTMPText.text = "";
    }
}
