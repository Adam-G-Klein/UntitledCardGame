using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionDataStore", 
    menuName = "Companions/Companion Data Store")]
public class CompanionDataStore : ScriptableObject
{
    public List<CompanionTypeSO> companions;
}
