using System;
using UnityEngine;

public class InstantiateFromClassname {
    public static T Instantiate<T>(string className, object[] args) {
        Type type = Type.GetType(className);
        if (type == null) {
            Debug.LogError("Could not find type " + className);
            return default(T);
        }
        Debug.Log("Instantiating " + type);
        foreach(object arg in args) {
            Debug.Log("\t" + arg);
        }
        return (T) Activator.CreateInstance(type, args);
    }
}