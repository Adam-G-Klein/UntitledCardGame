using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestSetupCompanions : MonoBehaviour
{
    public GameStateVariableSO gameStateVariableSO;
    public CardSelectionView cardSelectionView;

    [ContextMenu("Go")]
    public void Go() {
        List<Card> cards = new List<Card>();
        foreach (Companion companion in gameStateVariableSO.companions.activeCompanions) {
            cards.AddRange(companion.getDeck().cards);
        }
        cardSelectionView.Setup(cards, "Goobie Woobie", 1, 2, gameStateVariableSO.companions.activeCompanions[0]);
        // cardSelectionView.Setup(cards, "Goobie Woobie", 1, 2, null);
        cardSelectionView.cardsSelectedHandler += ((List<Card> cards) => {Debug.Log("CARDS SELECTED");});
    }
}
