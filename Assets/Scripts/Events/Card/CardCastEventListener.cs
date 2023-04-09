using UnityEngine.Events;

public class CardCastEventListener :
    BaseGameEventListener<
        CardCastEventInfo,
        CardCastEvent,
        UnityEvent<CardCastEventInfo>> { }