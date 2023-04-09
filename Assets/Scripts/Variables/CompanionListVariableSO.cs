using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionListVariable",
    menuName = "Companions/Companion List Variable")]
public class CompanionListVariableSO : ScriptableObject
{
    public List<Companion> companionList;
    public List<Companion> companionBench;
    public int currentCompanionSlots;
}
