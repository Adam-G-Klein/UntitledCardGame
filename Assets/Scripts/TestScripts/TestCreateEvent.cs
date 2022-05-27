using System.Collections;
using System.Collections.Generic;
using GameEventBus.Events;
using UnityEngine;

public class TestCreateEvent : MonoBehaviour
{
    public bool sendTestEvent = false;

    private TestEventManager eventManager;

    void Start()
    {
        eventManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<TestEventManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sendTestEvent)
        {
            eventManager.getEventBus().Publish(new RandomTestEvent("funny test event"));
            sendTestEvent = false;
        }
    }
}

class RandomTestEvent : EventBase
{
    private string testString;

    public RandomTestEvent(string testString)
    {
        this.testString = testString;
    }

    public string getTestString()
    {
        return testString;
    }
}
