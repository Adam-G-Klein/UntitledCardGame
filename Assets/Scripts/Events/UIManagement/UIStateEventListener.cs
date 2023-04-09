using UnityEngine.Events;

public class UIStateEventListener : 
    BaseGameEventListener<
        UIStateEventInfo,
        UIStateEvent,
        UnityEvent<UIStateEventInfo>> { }