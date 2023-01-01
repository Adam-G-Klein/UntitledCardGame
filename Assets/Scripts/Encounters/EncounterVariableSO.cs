using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EncounterReference : Reference<Encounter, EncounterVariableSO> {
    [SerializeReference]
    public Encounter ConstantValue;

    public EncounterReference(Encounter Value): base(Value) { }

    public EncounterReference() { }

    public Encounter Value
    {
        get { return UseConstant ? ConstantValue : Variable.Value; }
        set
        {
            if (UseConstant)
                ConstantValue = value;
            else
                Variable.Value = value;
        }
    }
}

[CreateAssetMenu(
    fileName = "EncounterVariable",
    menuName = "Encounters/Encounter Variable")]
public class EncounterVariableSO : VariableSO<Encounter> {
    [SerializeReference]
    public Encounter Value;
}