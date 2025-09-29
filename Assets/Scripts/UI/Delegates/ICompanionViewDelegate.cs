using System;
using System.Collections.Generic;
using UnityEngine;


public interface ICompanionViewDelegate {
    Sprite GetStatusEffectSprite(StatusEffectType statusEffectType);
    void ViewDeck(DeckViewType deckViewType, Companion companion = null, CompanionInstance companionInstance = null);
}