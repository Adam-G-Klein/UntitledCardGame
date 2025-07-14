using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;



[CreateAssetMenu(
    fileName = "NewAscensionSO",
    menuName = "ScriptableObjects/ascensionSO")]
[System.Serializable]
public class AscensionSO : ScriptableObject
{
    public string description;
    public string devDescription;
    public AscensionType ascensionType;
    public int modificationValue;
}

public enum AscensionType
{
    ENEMIES_DEADLIER,
    DAMAGED_COMPANIONS,
    STINGY_CONCIERGE,
    LESS_HEALTHY_UPGRADES,
    UNLUCKY_SHOPS,
    REDUCED_BENCH_CAPACITY,
    COSTLY_REROLLS,
    ALL_PACKS_ACTIVE
}