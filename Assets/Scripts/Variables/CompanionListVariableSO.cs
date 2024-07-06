using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionListVariable",
    menuName = "Companions/Companion List Variable")]
// TODO: make this actually a variableSO
public class CompanionListVariableSO : ScriptableObject
{
    public List<Companion> activeCompanions;
    public List<Companion> benchedCompanions;
    public List<Companion> allCompanions { get { 
        return activeCompanions.Concat(benchedCompanions).ToList(); } }
    public bool spaceInActiveCompanions { get { return activeCompanions.Count < currentCompanionSlots; } }
    public int currentCompanionSlots;

    public void SetCompanionSlots(int slots) {
        if (slots <= currentCompanionSlots) {
            Debug.LogWarning("New number of companion slots should be greater" +
            " than the current number. Ignoring the request to set companion slots");
            return;
        }
        currentCompanionSlots = slots;
    }

    public List<CompanionTypeSO> GetCompanionTypes() {
        List<CompanionTypeSO> companionTypes = new List<CompanionTypeSO>();
        foreach (Companion companion in activeCompanions) {
            companionTypes.Add(companion.companionType);
        }
        return companionTypes;
    }

    public void respawn() {
        activeCompanions = new List<Companion>();
        benchedCompanions = new List<Companion>();
        currentCompanionSlots = 3;
    }
}
