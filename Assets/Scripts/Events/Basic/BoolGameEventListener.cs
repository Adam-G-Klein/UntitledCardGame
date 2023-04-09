using UnityEngine.Events;

public class BoolGameEventListener :
    BaseGameEventListener<
        bool,
        BoolGameEvent,
        UnityEvent<bool>> {}
