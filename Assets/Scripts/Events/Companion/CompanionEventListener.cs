using UnityEngine.Events;

public class CompanionEventListener : 
    BaseGameEventListener<
        Companion,
        CompanionEvent,
        UnityEvent<Companion>> { }
