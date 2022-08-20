using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodedGameEventListener : IGameEventListener
{
    public GameEvent gameEvent;

    private Action response;

    

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable() {
        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response.Invoke();
    }
}
