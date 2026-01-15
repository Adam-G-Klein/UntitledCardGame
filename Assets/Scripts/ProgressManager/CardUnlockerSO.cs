using System.Collections.Generic;
using UnityEngine;

public abstract class CardUnlockerSO : ScriptableObject {
    public abstract List<CardType> ChooseCardsToUnlock(GameStateVariableSO gameState, List<CardType> unlockedCards);
}