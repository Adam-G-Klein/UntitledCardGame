using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEventListener<T, E, UER> : MonoBehaviour,
    IGameEventListener<T> where E: BaseGameEvent<T> where UER: UnityEvent<T> 
{
    [SerializeField]
    private E gameEvent;
    public E GameEvent {get { return gameEvent; } set { gameEvent = value; } }

    [SerializeField]
    private UER unityEventResponse;

    private void OnEnable()
    {
        if (gameEvent == null)
            return;
        
        GameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (gameEvent == null)
            return;
        
        GameEvent.UnregisterListener(this);
    }

    // The biggest thing we could actually serve to override
    // at some point, maybe providing our own OnEventRaised events
    public void OnEventRaised(T item)
    {
        if (unityEventResponse != null)
        {
            unityEventResponse.Invoke(item);
        }
    }
}
