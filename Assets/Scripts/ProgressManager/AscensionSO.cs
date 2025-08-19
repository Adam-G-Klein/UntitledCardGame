using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class AscensionKVPair
{
    public string Key;
    public float Value;
}

[Serializable]
public class AscensionValueDict
{
    [SerializeField] private List<AscensionKVPair> entries = new();

    private Dictionary<string, float> _dict;

    private Dictionary<string, float> ToDictionary()
    {
        if (_dict == null)
        {
            _dict = new Dictionary<string, float>();
            foreach (var entry in entries)
            {
                if (!_dict.ContainsKey(entry.Key))
                    _dict.Add(entry.Key, entry.Value);
            }
        }
        return _dict;
    }

    public float GetValueOrDefault(string key, float defaulVal)
    {
        var dict = ToDictionary();
        if (!dict.ContainsKey(key))
        {
            Debug.LogError("Ascension dict does not contain queried key \"" + key + "\". Please check the AscensionSO object");
        }
        return dict.GetValueOrDefault(key, defaulVal);
    }
}

[CreateAssetMenu(
    fileName = "NewAscensionSO",
    menuName = "ScriptableObjects/ascensionSO")]
[System.Serializable]
public class AscensionSO : ScriptableObject
{
    public string description;
    public string devDescription;
    public AscensionType ascensionType;
    [Header("Ascension key-values to modify the difficulty")]
    public AscensionValueDict ascensionModificationValues;
}

public enum AscensionType
{
    ENEMIES_DEADLIER,
    DAMAGED_COMPANIONS,
    STINGY_CONCIERGE,
    STINKIER_COMBINATION,
    SCARCE_SHOPS,
    REDUCED_BENCH_CAPACITY,
    COSTLY_REROLLS,
    WORSE_RATES_FOR_REBORN_RATS,
    ALL_PACKS_ACTIVE
}