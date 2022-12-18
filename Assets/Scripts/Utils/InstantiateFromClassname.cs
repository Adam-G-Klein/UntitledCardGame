using System;
using UnityEngine;

public class InstantiateFromClassname {
    public static T Instantiate<T>(string className, object[] args) {
        Type type = Type.GetType(className);
        if (type == null) {
            Debug.LogError("Could not find type " + className);
            return default(T);
        }
        return (T) Activator.CreateInstance(type, args);
    }
}