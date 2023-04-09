using UnityEngine.Events;

public class CardEffectEventListener : 
    BaseGameEventListener<
        CardEffectEventInfo,
        CardEffectEvent,
        UnityEvent<CardEffectEventInfo>> { }