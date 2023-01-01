using System;
using UnityEngine;

[Serializable]
public abstract class Reference { }

[Serializable]
public class Reference<T, G> : Reference where G : VariableSO<T>
{
    public bool UseConstant = false;

    public T ConstantValue;

    public G Variable;

    public Reference() { }
    public Reference(T value)
    {
        UseConstant = true;
        ConstantValue = value;
    }

    public T Value
    {
        get { return UseConstant ? ConstantValue : Variable.GetValue(); }
        set
        {
            if (UseConstant)
                ConstantValue = value;
            else
                Variable.SetValue(value);
        }
    }

    public static implicit operator T(Reference<T, G> Reference)
    {
        return Reference.Value;
    }

    public static implicit operator Reference<T, G>(T Value)
    {
        return new Reference<T, G>(Value);
    }
}