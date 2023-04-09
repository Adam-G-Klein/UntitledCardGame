using UnityEngine.Events;

public class CombatEffectEventListener : 
    BaseGameEventListener<
        CombatEffectEventInfo,
        CombatEffectEvent,
        UnityEvent<CombatEffectEventInfo>> { }