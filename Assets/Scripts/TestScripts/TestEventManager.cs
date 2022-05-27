using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;

public class TestEventManager : MonoBehaviour
{
    private EventBus eventBus = new EventBus();

    public EventBus getEventBus()
    {
        return eventBus;
    }
}
