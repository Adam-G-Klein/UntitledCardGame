using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class DeckDisplay : MonoBehaviour
{
    private CompanionInstance companionInstance;
    private TextMeshProUGUI drawPileCountText;
    private TextMeshProUGUI discardPileCountText;

    void Start() {
        companionInstance = GetComponentInParent<CompanionInstance>();
        drawPileCountText = transform.Find("DrawPileCount").GetComponent<TextMeshProUGUI>();
        discardPileCountText = transform.Find("DiscardPileCount").GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        drawPileCountText.text = companionInstance.inCombatDeck.drawPile.Count.ToString();
        discardPileCountText.text = companionInstance.inCombatDeck.discardPile.Count.ToString();
    }

}
