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

    public void SetCompanionSlots(int slots) {
        if (slots <= currentCompanionSlots) {
            Debug.LogWarning("New number of companion slots should be greater" +
            " than the current number. Ignoring the request to set companion slots");
            return;
        }
        currentCompanionSlots = slots;
    }
}
