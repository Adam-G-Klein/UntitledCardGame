using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPickupEvent : MonoBehaviour
{
    private TestEventManager eventManager;
    
    // Start is called before the first frame update
    void Start()
    {
        eventManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<TestEventManager>();
        eventManager.getEventBus().Subscribe<RandomTestEvent>(handleTestEvent);
    }

    void handleTestEvent(RandomTestEvent testEvent)
    {
        Debug.Log("handling the event in pickup event: " + testEvent.getTestString());
    }
}
