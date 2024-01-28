using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/*
    This will go through the map and for each kind of item under the input key,
    it will randomly select X of them and then output them to output key. If multiple
    types of items share the input key, this will happen for each kind of item.
*/
public class GetRandomItems : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private int scale = 1;
    [SerializeField]
    private string outputKey = "";
    public GetRandomItems() {
        effectStepName = "GetRandomItems";
    }

    public override IEnumerator invoke(EffectDocument document) {
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in document.map.GetDict()) {
            if (pair.Key.Item1 != inputKey) {
                continue;
            }

            List<object> items = RandomlyGetItems(pair.Value, scale);
            document.map.AddItemsOfType(outputKey, items, pair.Key.Item2);
        }
        yield return null;
    }

    private List<object> RandomlyGetItems(List<object> objs, int items) {
        if (items > objs.Count) {
            return objs;
        }
        List<object> returnList = new List<object>();
        List<object> listCopy = new List<object>(objs);
        for (int i = 0; i < items; i++) {
            int index = UnityEngine.Random.Range(0, listCopy.Count);
            returnList.Add(listCopy[index]);
            listCopy.RemoveAt(index);
        }
        return returnList;
    }
}
