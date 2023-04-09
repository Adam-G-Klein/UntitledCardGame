using UnityEngine.Events;

public class CardListEventListener : 
    BaseGameEventListener<
        CardListEventInfo,
        CardListEvent,
        UnityEvent<CardListEventInfo>> { }