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
    public int playersMaxAscensionUnlocked;
    public List<AscensionSO> ascensionSOList;
}