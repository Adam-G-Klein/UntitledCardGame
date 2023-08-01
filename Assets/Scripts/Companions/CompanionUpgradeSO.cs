using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionUpgrade",
    menuName = "Companions/Companion Upgrade")]
public class CompanionUpgradeSO : ScriptableObject
{
    public int healthUpgradeFactor;
    public int cardPerTurnUpgrade;
}
