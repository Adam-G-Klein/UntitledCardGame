using System;
using System.Collections;
using System.Collections.Generic;

public class UserDataWrapper
{
    private Dictionary<Type, object> _data = new();

    public void Set<T>(T value)
    {
        _data[typeof(T)] = value;
    }

    public T Get<T>() where T : class
    {
        if (_data.TryGetValue(typeof(T), out var value))
            return value as T;

        return null;
    }

    public bool Has<T>() where T : class => _data.ContainsKey(typeof(T));
}
