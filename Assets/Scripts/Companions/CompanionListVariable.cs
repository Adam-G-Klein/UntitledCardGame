using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionListVariable",
    menuName = "Companions/Companion List Variable")]
public class CompanionListVariable : ScriptableObject
{
    public List<Companion> companionList;
}
