using UnityEngine;
using TMPro;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class DeckDisplay : MonoBehaviour
{
    public TextMeshProUGUI drawPileCountText;
    public TextMeshProUGUI discardPileCountText;

    private DeckInstance deckInstance;

    void Start() {
        if (deckInstance == null)
            deckInstance = GetComponentInParent<DeckInstance>();
    }

    public void Setup(DeckInstance deckInstance) {
        this.deckInstance = deckInstance;
    }

    void Update() {
        drawPileCountText.text = deckInstance.drawPile.Count.ToString();
        discardPileCountText.text = deckInstance.discardPile.Count.ToString();
    }

}
