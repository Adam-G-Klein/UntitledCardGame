using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    public PlayerDataReference playerData;
    [Space(10)]
    public TMP_Text playerGoldTMPText;
    [Space(10)]
    public TMP_Text needMoreMoneyTMPText;
    public float needMoreMoneySeconds;
    [TextArea(1,5)]
    public string needMoreMoneyText;

    void Start() {
        needMoreMoneyTMPText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        playerGoldTMPText.text = playerData.Value.gold.ToString();
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
