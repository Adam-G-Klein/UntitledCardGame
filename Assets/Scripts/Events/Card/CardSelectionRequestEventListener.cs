using UnityEngine.Events;

public class CardSelectionRequestEventListener : 
    BaseGameEventListener<
        CardSelectionRequestEventInfo,
        CardSelectionRequestEvent,
        UnityEvent<CardSelectionRequestEventInfo>> { }