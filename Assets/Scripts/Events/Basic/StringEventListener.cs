using UnityEngine.Events;

public class StringEventListener : 
    BaseGameEventListener<
        string,
        StringGameEvent,
        UnityEvent<string>> { }