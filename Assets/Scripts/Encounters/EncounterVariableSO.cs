using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EncounterVariable",
    menuName = "Encounters/Encounter Variable")]
public abstract class EncounterVariableSO : ScriptableObject {
    [SerializeReference]
    public Encounter encounter;
}
