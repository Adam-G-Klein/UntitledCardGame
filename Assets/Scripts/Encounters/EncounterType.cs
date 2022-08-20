using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EncounterType :  ScriptableObject 
{
    public string encounterId;

    public abstract void Build();
}
