using UnityEngine.Events;

public class TargettableEntityListEventListener : 
    BaseGameEventListener<
        TargettableEntityListEventInfo,
        TargettableEntityListEvent,
        UnityEvent<TargettableEntityListEventInfo>> { }