using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EncounterTypeSO :  ScriptableObject 
{
    public string encounterId;

    public abstract void Build();
}
