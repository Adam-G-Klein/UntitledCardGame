using UnityEngine.Events;

public class CombatEntityDeathEventListener : 
    BaseGameEventListener<
        CombatEntityDeathEventInfo,
        CombatEntityDeathEvent,
        UnityEvent<CombatEntityDeathEventInfo>> { }