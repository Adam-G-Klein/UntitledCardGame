using UnityEngine.Events;

public class CardBuyRequestEventListener : 
    BaseGameEventListener<
        CardBuyRequest,
        CardBuyRequestEvent,
        UnityEvent<CardBuyRequest>> { }