using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableHashSet<T> : ISerializationCallbackReceiver, IEnumerable
{
    [SerializeField]
    private List<T> SetData = new();

    private HashSet<T> internalSet = new();

    public int Count { get => internalSet.Count(); }

    IEnumerator IEnumerable.GetEnumerator() {
        return internalSet.GetEnumerator();
    }

    public void OnBeforeSerialize() {
        foreach(T o in internalSet) {
            SetData.Add(o);
        }

        internalSet.Clear();
    }

    public void OnAfterDeserialize() {
        foreach(T o in SetData) {
            internalSet.Add(o);
        }

        SetData.Clear();
    }

    public bool Add(T o) {
        return internalSet.Add(o);
    }

    public bool Contains(T o) {
        return internalSet.Contains(o);
    }
}
