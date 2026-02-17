using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

/*
    This will go through the map and for each kind of item under the input key,
    it will randomly select X of them and then output them to output key. If multiple
    types of items share the input key, this will happen for each kind of item.
*/
public class GetRandomItems : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private int scale = 1;
    [SerializeField]
    private string scaleKey = "";
    [SerializeField]
    private bool getScaleFromKey = false;

    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    private bool withReplacement = false;

    public GetRandomItems() {
        effectStepName = "GetRandomItems";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<KeyValuePair<Tuple<string, Type>, List<object>>> mapCopy = document.map.GetDict().ToList();
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in mapCopy) {
            if (pair.Key.Item1 != inputKey) {
                continue;
            }

            if (getScaleFromKey) {
                scale = document.intMap.GetValueOrDefault(scaleKey, 0);
            }

            List<object> items = RandomlyGetItems(pair.Value, scale);
            document.map.AddItemsOfType(outputKey, items, pair.Key.Item2);
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    private List<object> RandomlyGetItems(List<object> objs, int items) {
        if (items > objs.Count && !withReplacement) {
            return objs;
        }
        List<object> returnList = new List<object>();
        List<object> listCopy = new List<object>(objs);
        for (int i = 0; i < items; i++) {
            int index = UnityEngine.Random.Range(0, listCopy.Count);
            returnList.Add(listCopy[index]);
            if (!withReplacement) {
                listCopy.RemoveAt(index);
            }
        }
        return returnList;
    }
}
