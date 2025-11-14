using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityEventDispatcher<TDelegate> where TDelegate : Delegate
{
    private readonly List<(int priority, TDelegate handler)> handlers = new();

    public void AddHandler(TDelegate handler, int priority)
    {
        handlers.Add((priority, handler));
        handlers.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public void RemoveHandler(TDelegate handler)
    {
        handlers.RemoveAll(h => h.handler == handler);
    }

    public IEnumerable<IEnumerator> Invoke(params object[] args)
    {
        foreach (var (priority, handler) in handlers)
        {
            // handler.DynamicInvoke(args) returns an object; cast to IEnumerator
            yield return (IEnumerator)handler.DynamicInvoke(args);
        }
    }
}
