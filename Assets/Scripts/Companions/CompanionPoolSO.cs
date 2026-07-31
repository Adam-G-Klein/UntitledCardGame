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

    public bool ContainsRat(CompanionTypeSO rat) {
        return commonCompanions.Contains(rat) || uncommonCompanions.Contains(rat) || rareCompanions.Contains(rat);
    }
}

/*
    Base C# version of the pool SO, meant to be used if we want to hold a "combined" pool
    or a pool with changes made at runtime.
*/
[System.Serializable]
public class CompanionPool {
    public List<CompanionTypeSO> commonCompanions;
    public List<CompanionTypeSO> uncommonCompanions;
    public List<CompanionTypeSO> rareCompanions;
}