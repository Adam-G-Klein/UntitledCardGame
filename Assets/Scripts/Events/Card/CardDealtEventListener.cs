using UnityEngine.Events;

public class CardDealtEventListener : 
    BaseGameEventListener<
        CardsDealtEventInfo,
        CardsDealtEvent,
        UnityEvent<CardsDealtEventInfo>> { }