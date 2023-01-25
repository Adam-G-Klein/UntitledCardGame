using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class DeckDisplay : MonoBehaviour
{
    private CombatEntityWithDeckInstance combatEntityWithDeckInstance;
    private TextMeshProUGUI drawPileCountText;
    private TextMeshProUGUI discardPileCountText;

    void Start() {
        combatEntityWithDeckInstance = GetComponentInParent<CombatEntityWithDeckInstance>();
        drawPileCountText = transform.Find("DrawPileCount").GetComponent<TextMeshProUGUI>();
        discardPileCountText = transform.Find("DiscardPileCount").GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        drawPileCountText.text = combatEntityWithDeckInstance.inCombatDeck.drawPile.Count.ToString();
        discardPileCountText.text = combatEntityWithDeckInstance.inCombatDeck.discardPile.Count.ToString();
    }

}
