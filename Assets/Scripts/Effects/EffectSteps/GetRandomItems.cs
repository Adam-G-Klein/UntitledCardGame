using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

/*
    This will go through the map and, for the items under the input key, randomly
    select X of them and output them to the output key.

    A single entity is usually stored under a key as several parallel typed lists
    (e.g. EffectUtils.AddCompanionToDocument writes CompanionInstance, CombatInstance,
    DeckInstance, GameObject, ... at the same index). To keep those lists aligned, the
    random indices are chosen once per list length and applied to every list of that
    length. Otherwise each type would pick different entities, and downstream steps
    reading different types (e.g. DrawCards vs InstantiatePrefab) would disagree.
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
        int finalScale = scale;
        if (getScaleFromKey) {
            document.intMap.TryGetValue(scaleKey, out finalScale);
        }

        // Snapshot the matching lists first, since we write back into the same map.
        List<KeyValuePair<Tuple<string, Type>, List<object>>> matching = document.map.GetDict()
            .Where(pair => pair.Key.Item1 == inputKey)
            .ToList();

        // Parallel lists for the same set of entities share a length, so pick one
        // set of indices per length and apply it to every list in that group.
        Dictionary<int, List<int>> indicesByCount = new Dictionary<int, List<int>>();
        foreach (KeyValuePair<Tuple<string, Type>, List<object>> pair in matching) {
            List<object> items = pair.Value;
            if (!indicesByCount.TryGetValue(items.Count, out List<int> indices)) {
                indices = RandomIndices(items.Count, finalScale);
                indicesByCount[items.Count] = indices;
            }

            // TEMP DEBUG: remove once the Sift draw-count issue is understood.
            // EffectLog(" type=" + pair.Key.Item2.Name + " count=" + items.Count +
                // " scale=" + finalScale + " picked=[" + string.Join(",", indices) + "]");

            List<object> selected = indices.Select(i => items[i]).ToList();
            document.map.AddItemsOfType(outputKey, selected, pair.Key.Item2);
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    private List<int> RandomIndices(int count, int n) {
        if (n > count && !withReplacement) {
            return Enumerable.Range(0, count).ToList();
        }
        List<int> returnList = new List<int>();
        if (count == 0) {
            return returnList;
        }
        List<int> pool = Enumerable.Range(0, count).ToList();
        for (int i = 0; i < n; i++) {
            int index = UnityEngine.Random.Range(0, pool.Count);
            returnList.Add(pool[index]);
            if (!withReplacement) {
                pool.RemoveAt(index);
            }
        }
        return returnList;
    }
}
