using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class DeckDisplay : MonoBehaviour
{
    private DeckInstance deckInstance;
    private TextMeshProUGUI drawPileCountText;
    private TextMeshProUGUI discardPileCountText;

    void Start() {
        deckInstance = GetComponentInParent<DeckInstance>();
        drawPileCountText = transform.Find("DrawPileCount").GetComponent<TextMeshProUGUI>();
        discardPileCountText = transform.Find("DiscardPileCount").GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        drawPileCountText.text = deckInstance.drawPile.Count.ToString();
        discardPileCountText.text = deckInstance.discardPile.Count.ToString();
    }

}
