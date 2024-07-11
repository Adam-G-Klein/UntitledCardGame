using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCompanionPool",
    menuName = "Companions/Companion Pool")]
public class CompanionPoolSO: ScriptableObject {

    public List<CompanionTypeSO> commonCompanions;
    public List<CompanionTypeSO> uncommonCompanions;
    public List<CompanionTypeSO> rareCompanions;
}