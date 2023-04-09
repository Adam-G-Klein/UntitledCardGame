using UnityEngine.Events;

public class TurnPhaseEventListener : 
    BaseGameEventListener<
        TurnPhaseEventInfo,
        TurnPhaseEvent,
        UnityEvent<TurnPhaseEventInfo>> { }