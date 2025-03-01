using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EncounterVariable",
    menuName = "Encounters/Encounter Variable")]
[System.Serializable]
public class EncounterVariableSO : ScriptableObject {
    public bool locked = false;
    [SerializeReference]
    public Encounter Value;

    public Encounter GetValue() {
        return Value;
    }

    public void SetValue(Encounter value)
    {
        if (!locked)
            Value = value;
    }

    public void SetValue(EncounterVariableSO value)
    {
        if (!locked)
            Value = value.Value;
    }
}