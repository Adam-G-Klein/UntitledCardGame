using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameEvent<T> : ScriptableObject
{
    private bool debugLogEvents = false;

	private readonly List<IGameEventListener<T>> listeners = 
		new List<IGameEventListener<T>>();

    public void Raise(T item)
    {
        if (debugLogEvents) {
            Debug.Log("GameEvent raised: " + item);
        }
        
        for(int i = listeners.Count -1; i >= 0; i--)
            listeners[i].OnEventRaised(item);
    }

    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(IGameEventListener<T> listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }

    public IEnumerator RaiseAtEndOfFrameCoroutine(T item)
    {
        yield return new WaitForEndOfFrame();
        Raise(item);
    }
}
