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
        Companion companion = gameStateVariableSO.companions.activeCompanions[0];
        cardSelectionView.Setup(companion.getDeck().cards);
    }
}
