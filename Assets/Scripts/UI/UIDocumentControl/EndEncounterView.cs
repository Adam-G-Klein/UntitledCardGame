using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EndEncounterView : MonoBehaviour
{
    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
    }

    public void Setup(int baseGoldEarnedPerBattle, int interestEarned, int interestCap, float interestPercentage)
    {
        doc.rootVisualElement.Q<Label>("base-gold").text = "Base Gold Earned: " + baseGoldEarnedPerBattle.ToString();
        doc.rootVisualElement.Q<Label>("interest").text = "Interest: " + interestEarned.ToString();
        doc.rootVisualElement.Q<Label>("interest-help").text = "(You earn " +
            interestPercentage.ToString("P0") + 
            " of your current Gold as Interest, capped at " +
            interestCap.ToString() + " gold per combat)";
    }

    /*
    "Base gold earned\n$" +
                baseGoldEarnedPerBattle.ToString() +
                "\ninterest (" +
                gameState.baseShopData.interestRate.ToString("P0") +
                ", capped at $" +
                gameState.baseShopData.interestCap.ToString() +
                ")\n$" +
                extraGold.ToString();
                */

}
