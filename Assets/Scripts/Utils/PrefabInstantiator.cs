using System.Collections.Generic;
using UnityEngine;

public static class PrefabInstantiator {

    //We're probably going to end up doing a lot of this
    //I'm willing to bet something fancy with generic types involved
    //will be a move. Not gonna do that until we write our second function like this though
    public static PlayableCard instantiateCard(GameObject cardPrefab, Transform parent, CardInfo card, Companion companionFrom){
        GameObject newCard = GameObject.Instantiate(cardPrefab, parent);
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        PlayableCard cardPlayable = newCard.GetComponent<PlayableCard>();
        cardDisplay.cardInfo = card;
        cardPlayable.setCardInfo(card);
        cardPlayable.setCompanionFrom(companionFrom);
        return cardPlayable;
    }
}