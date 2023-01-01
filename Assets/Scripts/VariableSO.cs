using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableSO<T> : ScriptableObject {
    [SerializeField]
    private T Value;
    public bool locked = false;

    public T GetValue() {
        return Value;
    }

    public void SetValue(T value)
    {
        if (!locked)
            Value = value;
    }

    public void SetValue(VariableSO<T> value)
    {
        if (!locked)
            Value = value.Value;
    }
}