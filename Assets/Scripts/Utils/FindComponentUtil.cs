using System;
using UnityEngine;

public class FindComponentUtil {
    public static T FindComponentInParents<T>(Transform t) {
        T component = t.GetComponent<T>();
        if (component != null) {
            return component;
        }
        if (t.parent == null) {
            return default(T);
        }
        return FindComponentInParents<T>(t.parent);
    }
}