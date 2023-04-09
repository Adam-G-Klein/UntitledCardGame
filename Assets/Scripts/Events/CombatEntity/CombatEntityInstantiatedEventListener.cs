using UnityEngine.Events;

public class CombatEntityInstantiatedEventListener : 
    BaseGameEventListener<
        CombatEntityInstantiatedEventInfo,
        CombatEntityInstantiatedEvent,
        UnityEvent<CombatEntityInstantiatedEventInfo>> { }