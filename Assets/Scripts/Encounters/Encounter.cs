using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Encounter
{
    public EncounterTypeSO encounterType;
    public bool completed;

    public void Build() 
    {
        if (!completed)
            encounterType.Build();
    }
}
