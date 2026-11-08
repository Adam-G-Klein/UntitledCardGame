using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAscensionInfoSO",
    menuName = "ScriptableObjects/ascensionInfoSO")]
[System.Serializable]
public class AscensionInfo : ScriptableObject
{
    // Saves record this so ascension indexes from before base difficulty was added at index 0 can be shifted up by one
    public const int AscensionIndexVersion = 1;

    public int playersMaxAscensionUnlocked;
    public List<AscensionSO> ascensionSOList;
}